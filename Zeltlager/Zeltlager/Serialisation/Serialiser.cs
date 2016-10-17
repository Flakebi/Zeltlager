using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zeltlager.Serialisation
{
    public class Serialiser
    {
        class FieldData
        {
            public string Name;
            public Type Type;
            public object Value;
            public SerialisationAttribute Attribute;

            public FieldData(string name, Type type, object value, SerialisationAttribute attribute)
            {
                Name = name;
                Type = type;
                Value = value;
                Attribute = attribute;
            }
        }

        static readonly Dictionary<Type, string> PRIMITIVES = new Dictionary<Type, string>
        {
            { typeof(string), "String" },
            { typeof(bool), "Boolean" },
			// Numbers
			{ typeof(byte), "Byte" },
            { typeof(sbyte), "SByte" },
            { typeof(ushort), "UInt16" },
            { typeof(short), "Int16" },
            { typeof(uint), "UInt32" },
            { typeof(int), "Int32" },
            { typeof(ulong), "UInt64" },
            { typeof(long), "Int64" },
            { typeof(float), "Single" },
            { typeof(double), "Double" },
        };

        IEnumerable<FieldData> GetFieldData(TypeInfo typeInfo, object obj)
        {
            var fields = typeInfo.DeclaredFields
                .Where(f => f.GetCustomAttribute<SerialisationAttribute>() != null)
                .Select(f => new FieldData(f.Name, f.FieldType,
                obj == null ? (f.FieldType.GetTypeInfo().IsValueType ? Activator.CreateInstance(f.FieldType) : null) : f.GetValue(obj),
                f.GetCustomAttribute<SerialisationAttribute>()));
            var properties = typeInfo.DeclaredProperties
                .Where(p => p.GetCustomAttribute<SerialisationAttribute>() != null)
                .Select(p => new FieldData(p.Name, p.PropertyType,
                obj == null ? (p.PropertyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(p.PropertyType) : null) : p.GetValue(obj),
                p.GetCustomAttribute<SerialisationAttribute>()));
            // Sort by name
            return fields.Concat(properties).OrderBy(t => t.Name);
        }

        public void Write<T>(BinaryWriter output, SerialisationContext context, T obj)
        {
            var type = typeof(T);
            TypeInfo typeInfo = type.GetTypeInfo();
            // Check if the object implements ISerialisable
            ISerialisable serialisable = obj as ISerialisable;
            if (serialisable != null)
                serialisable.Write(output, context);
            else if (PRIMITIVES.ContainsKey(type))
            {
                // Check for primitives
                // Search for the matching method
                typeof(BinaryWriter).GetTypeInfo().GetDeclaredMethods(nameof(Write)).First(m =>
                {
                    var arguments = m.GetParameters();
                    return arguments.Length == 1 && arguments[0].ParameterType == typeof(T);
                }).Invoke(output, new object[] { obj });
            } else if (typeInfo.ImplementedInterfaces.Contains(typeof(IList)))
            {
                // Check for lists and arrays
                var list = (IList)obj;
                output.Write(list.Count);
                // Get the type parameter if it is a generic list or the element
                Type elementType = typeof(object);
                if (typeInfo.IsGenericType)
                    elementType = typeInfo.GenericTypeParameters[0];
                else if (typeInfo.IsArray)
                    elementType = typeInfo.GetElementType();
                else
                    elementType = typeof(object);

                // Get the generic method
                var method = typeof(Serialiser).GetTypeInfo().GetDeclaredMethod(nameof(Write));
                method = method.MakeGenericMethod(elementType);
                // Serialise all elements
                foreach (var o in list)
                    method.Invoke(this, new object[] { output, context, o });
            } else
            {
                // Collect attributes in an array
                var attributes = GetFieldData(typeInfo, obj).Where(a => a.Attribute.Type != SerialisationType.Id).ToArray();
                foreach (var attribute in attributes)
                    WriteField(output, context, attribute);
            }
        }

        public void WriteId<T>(BinaryWriter output, SerialisationContext context, T obj)
        {
            // Check if the object implements ISerialisable
            ISerialisable serialisable = obj as ISerialisable;
            if (serialisable != null)
                serialisable.WriteId(output, context);
            else
            {
                TypeInfo typeInfo = typeof(T).GetTypeInfo();
                // Find the id of the type
                var attributes = GetFieldData(typeInfo, obj).Where(a => a.Attribute.Type == SerialisationType.Id).ToArray();
                foreach (var attribute in attributes)
                    WriteField(output, context, attribute);
            }
        }

        void WriteField(BinaryWriter output, SerialisationContext context, FieldData attribute)
        {
            // Check if the attribute is optional
            if (attribute.Attribute.Optional)
            {
                if (attribute.Value == null)
                {
                    output.Write(false);
                    return;
                } else
                    output.Write(true);
            }
            // Check if we should only save a reference
            string methodName = attribute.Attribute.Type == SerialisationType.Reference ? nameof(WriteId) : nameof(Write);
            // Use the previously taken type as the generic parameter for the method call
            var method = typeof(Serialiser).GetTypeInfo().GetDeclaredMethod(methodName);
            method = method.MakeGenericMethod(attribute.Type);
            method.Invoke(this, new object[] { output, context, attribute.Value });
        }


        public T Read<T>(BinaryReader input, SerialisationContext context)
        {
            var type = typeof(T);
            TypeInfo typeInfo = type.GetTypeInfo();
            // Check if the object implements ISerialisable
            if (typeInfo.ImplementedInterfaces.Contains(typeof(ISerialisable)))
            {
                // Find a matching method
                var method = typeInfo.GetDeclaredMethods(nameof(Read)).First(m =>
                {
                    if (!m.IsStatic)
                        return false;
                    var arguments = m.GetParameters();
                    if (arguments.Length != 2)
                        return false;
                    return arguments[0].ParameterType == typeof(BinaryReader)
                        && arguments[1].ParameterType == typeof(SerialisationContext);
                });
                return (T)method.Invoke(null, new object[] { input, context });
            } else if (PRIMITIVES.ContainsKey(type))
            {
                // Check for primitives
                // Search for the matching method
                return (T)typeof(BinaryReader).GetTypeInfo().GetDeclaredMethods(nameof(Read) + PRIMITIVES[type])
                    .First(m => m.GetParameters().Length == 0).Invoke(input, new object[0]);
            } else if (typeInfo.ImplementedInterfaces.Contains(typeof(IList)))
            {
                // Check for lists and arrays
                int count = input.ReadInt32();
                T result = default(T);
                // Try to call the constructor that takes the capacity
                var constructor = typeInfo.DeclaredConstructors.Where(c =>
                {
                    var arguments = c.GetParameters();
                    return arguments.Length == 0 ||
                        (arguments.Length == 1 && arguments[0].ParameterType == typeof(int));
                }).OrderBy(c => 1 - c.GetParameters().Length).First();
                if (constructor.GetParameters().Length == 1)
                    result = (T)constructor.Invoke(new object[] { count });
                else
                    result = (T)constructor.Invoke(new object[0]);
                IList list = (IList)result;

                // Get the type parameter if it is a generic list or the element
                Type elementType = typeof(object);
                if (typeInfo.IsGenericType)
                    elementType = typeInfo.GenericTypeParameters[0];
                else if (typeInfo.IsArray)
                    elementType = typeInfo.GetElementType();
                else
                    elementType = typeof(object);

                // Get the generic method
                var method = typeof(Serialiser).GetTypeInfo().GetDeclaredMethod(nameof(Read));
                method = method.MakeGenericMethod(elementType);
                // Read list elements
                for (int i = 0; i < count; i++)
                {
                    var obj = method.Invoke(this, new object[] { input, context });
                    if (list.IsFixedSize)
                        list[i] = obj;
                    else
                        list.Add(obj);
                }

                return result;
            } else
            {
                // Collect attributes in an array
                var attributes = GetFieldData(typeInfo, null).Where(a => a.Attribute.Type != SerialisationType.Id).ToArray();
                foreach (var attribute in attributes)
                    ReadField(input, context, attribute);

                return (T)CallConstructor(typeInfo, attributes, context);
            }
        }

        public T ReadFromId<T>(BinaryReader input, SerialisationContext context)
        {
            // Check if the object implements ISerialisable
            TypeInfo typeInfo = typeof(T).GetTypeInfo();
            if (typeInfo.ImplementedInterfaces.Contains(typeof(ISerialisable)))
            {
                // Find a matching method
                var method = typeInfo.GetDeclaredMethods(nameof(ReadFromId)).First(m =>
                {
                    if (!m.IsStatic)
                        return false;
                    var arguments = m.GetParameters();
                    if (arguments.Length != 2)
                        return false;
                    return arguments[0].ParameterType == typeof(BinaryReader)
                        && arguments[1].ParameterType == typeof(SerialisationContext);
                });
                return (T)method.Invoke(null, new object[] { input, context });
            } else
            {
                // Find the id of the type
                var attributes = GetFieldData(typeInfo, null).Where(a => a.Attribute.Type == SerialisationType.Id).ToArray();
                foreach (var attribute in attributes)
                    ReadField(input, context, attribute);

                return (T)CallConstructor(typeInfo, attributes, context);
            }
        }

        /// <summary>
        /// Read the value of a field into the value field of the attribute.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="context">The context.</param>
        /// <param name="attribute">
        /// The attribute that contains information about the value that
        /// should be read and that will contain the read value.
        /// </param>
        void ReadField(BinaryReader input, SerialisationContext context, FieldData attribute)
        {
            // Check if the attribute is optional
            if (attribute.Attribute.Optional)
            {
                if (!input.ReadBoolean())
                    return;
            }
            // Check if we should read a reference
            string methodName = attribute.Attribute.Type == SerialisationType.Reference ? nameof(ReadFromId) : nameof(Read);
            // Use the previously taken type as the generic parameter for the method call
            var method = typeof(Serialiser).GetTypeInfo().GetDeclaredMethod(methodName);
            method = method.MakeGenericMethod(attribute.Type);
            attribute.Value = method.Invoke(this, new object[] { input, context });
        }

        object CallConstructor(TypeInfo typeInfo, FieldData[] attributes, SerialisationContext context)
        {
            // Search for a fitting constructor
            foreach (var constructor in typeInfo.DeclaredConstructors)
            {
                List<FieldData> remainingAttributes = new List<FieldData>(attributes);
                var parameters = constructor.GetParameters();
                object[] arguments = new object[parameters.Length];
                bool success = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.ParameterType == typeof(SerialisationContext))
                        arguments[i] = context;
                    else
                    {
                        if (!parameters.Skip(i + 1).Select(p => p.ParameterType).Contains(parameter.ParameterType)
                            && remainingAttributes.Count(a => a.Type == parameter.ParameterType) == 1)
                        {
                            // Check if the type of this parameter is unambiguous
                            int index = remainingAttributes.FindIndex(a => a.Type == parameter.ParameterType);
                            arguments[i] = remainingAttributes[index].Value;
                            remainingAttributes.RemoveAt(index);
                            continue;
                        } else
                        {
                            // Take the attribute with the same name if it exists
                            int index = remainingAttributes.FindIndex(a => a.Name.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase));
                            if (index != -1)
                            {
                                arguments[i] = remainingAttributes[index].Value;
                                remainingAttributes.RemoveAt(index);
                                continue;
                            }
                        }

                        success = false;
                        break;
                    }
                }
                if (success && remainingAttributes.Count == 0)
                    return constructor.Invoke(arguments);
            }
            throw new NotImplementedException("The type does not specify a constructor suitable for deserialisation");
        }
    }
}
