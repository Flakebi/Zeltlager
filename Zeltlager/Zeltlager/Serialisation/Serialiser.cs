using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zeltlager.Serialisation
{
	/// <summary>
	/// A serialiser that can serialise and deserialise objects
	/// with a certain context.
	/// </summary>
	/// <typeparam name="C">The context for the serialisation.</typeparam>
	public class Serialiser<C>
	{
		class FieldData
		{
			public string Name;
			public Type Type;
			public object Value;
			public SerialisationAttribute Attribute;

			public FieldInfo Field;
			public PropertyInfo Property;

			public FieldData(string name, Type type, object value, SerialisationAttribute attribute)
			{
				Name = name;
				Type = type;
				Value = value;
				Attribute = attribute;
			}

			public FieldData(string name, Type type, object value, SerialisationAttribute attribute, FieldInfo field) :
				this(name, type, value, attribute)
			{
				Field = field;
			}

			public FieldData(string name, Type type, object value, SerialisationAttribute attribute, PropertyInfo property) :
				this(name, type, value, attribute)
			{
				Property = property;
			}

			public FieldData(FieldInfo field, object obj) :
				this(field.Name, field.FieldType,
					obj == null ? GetDefault(field.FieldType) : field.GetValue(obj),
					field.GetCustomAttribute<SerialisationAttribute>(), field)
			{
			}

			public FieldData(PropertyInfo property, object obj) :
				this(property.Name, property.PropertyType,
					obj == null ? GetDefault(property.PropertyType) : property.GetValue(obj),
					property.GetCustomAttribute<SerialisationAttribute>(), property)
			{
			}

			public override string ToString()
			{
				return Name;
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

		static IEnumerable<FieldData> GetFieldData(Type type, object obj)
		{
			var fields = type.GetRuntimeFields()
				.Where(f => f.GetCustomAttribute<SerialisationAttribute>() != null)
				.Select(f => new FieldData(f, obj));
			var properties = type.GetRuntimeProperties()
				.Where(p => p.GetCustomAttribute<SerialisationAttribute>() != null)
				.Select(p => new FieldData(p, obj));
			// Sort by name
			return fields.Concat(properties).OrderBy(t => t.Name);
		}

		static object GetDefault(Type type)
		{
			return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
		}

		/// <summary>
		/// Write the given object to the output stream.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be written.</typeparam>
		/// <param name="output">The output stream for the serialisation.</param>
		/// <param name="context">The context for user defined serialisations.</param>
		/// <param name="obj">The object that should be written into output.</param>
		public void Write<T>(BinaryWriter output, C context, T obj)
		{
			var type = typeof(T);
			TypeInfo typeInfo = type.GetTypeInfo();
			// Check if the object implements ISerialisable
			ISerialisable<C> serialisable = obj as ISerialisable<C>;
			if (serialisable != null)
				serialisable.Write(output, this, context);
			else if (PRIMITIVES.ContainsKey(type))
			{
				// Check for primitives
				// Search for the matching method
				typeof(BinaryWriter).GetRuntimeMethod(nameof(Write), new Type[] { typeof(T) })
					.Invoke(output, new object[] { obj });
			} else if (typeInfo.ImplementedInterfaces.Contains(typeof(IList)))
			{
				// Check for lists and arrays
				var list = (IList)obj;
				output.Write(list.Count);
				// Get the type parameter if it is a generic list or the element
				Type elementType = typeof(object);
				if (typeInfo.IsGenericType)
					elementType = typeInfo.GenericTypeArguments[0];
				else if (typeInfo.IsArray)
					elementType = typeInfo.GetElementType();
				else
					elementType = typeof(object);

				// Get the generic method
				var method = typeof(Serialiser<C>).GetTypeInfo().GetDeclaredMethod(nameof(Write));
				method = method.MakeGenericMethod(elementType);
				// Serialise all elements
				foreach (var o in list)
					method.Invoke(this, new object[] { output, context, o });
			} else
			{
				// Collect attributes in an array
				var attributes = GetFieldData(type, obj).ToArray();
				foreach (var attribute in attributes)
					WriteField(output, context, attribute);
			}
		}

		/// <summary>
		/// Write the id of a given object to the output stream.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be written.</typeparam>
		/// <param name="output">The output stream for the serialisation.</param>
		/// <param name="context">The context for user defined serialisations.</param>
		/// <param name="obj">The object of which the id should be written into output.</param>
		public void WriteId<T>(BinaryWriter output, C context, T obj)
		{
			// Check if the object implements ISerialisable
			ISerialisable<C> serialisable = obj as ISerialisable<C>;
			if (serialisable != null)
				serialisable.WriteId(output, this, context);
			else
			{
				// Find the id of the type
				var attributes = GetFieldData(typeof(T), obj).Where(a => a.Attribute.Type == SerialisationType.Id).ToArray();
				if (attributes.Length == 0)
				{
					// Check if it's a list of ids
					TypeInfo typeInfo = typeof(T).GetTypeInfo();
					if (typeInfo.ImplementedInterfaces.Contains(typeof(IList)))
					{
						// Check for lists and arrays
						var list = (IList)obj;
						output.Write(list.Count);
						// Get the type parameter if it is a generic list or the element
						Type elementType = typeof(object);
						if (typeInfo.IsGenericType)
							elementType = typeInfo.GenericTypeArguments[0];
						else if (typeInfo.IsArray)
							elementType = typeInfo.GetElementType();
						else
							elementType = typeof(object);

						// Get the generic method
						var method = typeof(Serialiser<C>).GetTypeInfo().GetDeclaredMethod(nameof(WriteId));
						method = method.MakeGenericMethod(elementType);
						// Serialise all elements
						foreach (var o in list)
							method.Invoke(this, new object[] { output, context, o });
					}
				} else
				{
					foreach (var attribute in attributes)
						WriteField(output, context, attribute);
				}
			}
		}

		void WriteField(BinaryWriter output, C context, FieldData attribute)
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
			var method = typeof(Serialiser<C>).GetTypeInfo().GetDeclaredMethod(methodName);
			method = method.MakeGenericMethod(attribute.Type);
			method.Invoke(this, new object[] { output, context, attribute.Value });
		}

		/// <summary>
		/// Read an object from the given input stream and store the data in the given object.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be read.</typeparam>
		/// <param name="input">The input stream for the serialisation.</param>
		/// <param name="context">The context for user defined serialisations.</param>
		/// <param name="obj">The object that should be filled with the read data.</param>
		/// <returns>The read object.</returns>
		public T Read<T>(BinaryReader input, C context, T obj)
		{
			var type = typeof(T);
			TypeInfo typeInfo = type.GetTypeInfo();
			// Check if the object implements ISerialisable
			ISerialisable<C> serialisable = obj as ISerialisable<C>;
			if (serialisable != null)
				serialisable.Read(input, this, context);
			else if (PRIMITIVES.ContainsKey(type))
			{
				// Check for primitives
				// Search for the matching method
				obj = (T)typeof(BinaryReader).GetRuntimeMethod(nameof(Read) + PRIMITIVES[type], new Type[0])
					.Invoke(input, new object[0]);
			} else if (typeInfo.ImplementedInterfaces.Contains(typeof(IList)))
			{
				// Check for lists and arrays
				int count = input.ReadInt32();
				// Create the object if it doesn't exist yet
				if (obj == null)
				{
					// Try to call the constructor that takes the capacity
					var constructor = typeInfo.DeclaredConstructors.Where(c =>
					{
						var arguments = c.GetParameters();
						return arguments.Length == 0 ||
							(arguments.Length == 1 && arguments[0].ParameterType == typeof(int));
					}).OrderBy(c => 1 - c.GetParameters().Length).First();
					if (constructor.GetParameters().Length == 1)
						obj = (T)constructor.Invoke(new object[] { count });
					else
						obj = (T)constructor.Invoke(new object[0]);
				}
				IList list = (IList)obj;

				// Get the type parameter if it is a generic list or the element
				Type elementType = typeof(object);
				if (typeInfo.IsGenericType)
					elementType = typeInfo.GenericTypeArguments[0];
				else if (typeInfo.IsArray)
					elementType = typeInfo.GetElementType();
				else
					elementType = typeof(object);

				// Get the generic method
				var method = typeof(Serialiser<C>).GetTypeInfo().GetDeclaredMethod(nameof(Read));
				method = method.MakeGenericMethod(elementType);
				// Read list elements
				for (int i = 0; i < count; i++)
				{
					if (list.Count > i)
						list[i] = method.Invoke(this, new object[] { input, context, list[i] });
					else
						list.Add(method.Invoke(this, new object[] { input, context, null }));
				}
			} else
			{
				// Collect attributes in an array
				var attributes = GetFieldData(type, obj).ToArray();
				// Read all attributes and set the fields of the object
				foreach (var attribute in attributes)
				{
					ReadField(input, context, attribute);
					if (attribute.Field != null)
						attribute.Field.SetValue(obj, attribute.Value);
					else
						attribute.Property.SetValue(obj, attribute.Value);
				}
			}
			return obj;
		}

		/// <summary>
		/// Get an object from the id that is read from the given input stream.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be read.</typeparam>
		/// <param name="input">The input stream for the serialisation.</param>
		/// <param name="context">The context for user defined serialisations.</param>
		/// <returns>The object of the read id.</returns>
		public T ReadFromId<T>(BinaryReader input, C context)
		{
			// Check if the object implements ISerialisable
			Type type = typeof(T);
			TypeInfo typeInfo = type.GetTypeInfo();
			if (typeInfo.ImplementedInterfaces.Contains(typeof(ISerialisable<C>)))
			{
				// Find a matching method
				var method = typeInfo.GetDeclaredMethods(nameof(ReadFromId))
					.First(m => m.IsStatic && m.GetParameters().Select(
						p => p.ParameterType).SequenceEqual(new Type[] { typeof(BinaryReader), typeof(Serialiser<C>), typeof(C) }));
				return (T)method.Invoke(null, new object[] { input, this, context });
			} else
			{
				// Find the id of the type
				var attributes = GetFieldData(type, null).Where(a => a.Attribute.Type == SerialisationType.Id).ToArray();
				if (attributes.Length == 0)
				{
					// Check if it's a list of ids
					if (typeInfo.ImplementedInterfaces.Contains(typeof(IList)))
					{
						T obj;
						// Check for lists and arrays
						int count = input.ReadInt32();
						// Create the object
						// Try to call the constructor that takes the capacity
						var constructor = typeInfo.DeclaredConstructors.Where(c =>
						{
							var arguments = c.GetParameters();
							return arguments.Length == 0 ||
								(arguments.Length == 1 && arguments[0].ParameterType == typeof(int));
						}).OrderBy(c => 1 - c.GetParameters().Length).First();
						if (constructor.GetParameters().Length == 1)
							obj = (T)constructor.Invoke(new object[] { count });
						else
							obj = (T)constructor.Invoke(new object[0]);

						IList list = (IList)obj;
						// Get the type parameter if it is a generic list or the element
						Type elementType = typeof(object);
						if (typeInfo.IsGenericType)
							elementType = typeInfo.GenericTypeArguments[0];
						else if (typeInfo.IsArray)
							elementType = typeInfo.GetElementType();
						else
							elementType = typeof(object);

						// Get the generic method
						var method = typeof(Serialiser<C>).GetTypeInfo().GetDeclaredMethod(nameof(ReadFromId));
						method = method.MakeGenericMethod(elementType);
						// Serialise all elements
						foreach (var o in list)
							method.Invoke(this, new object[] { input, context, o });

						return obj;
					}
				} else
				{
					// Read the id
					foreach (var attribute in attributes)
						ReadField(input, context, attribute);
				}

				return (T)CallGetFromIdMethod(typeInfo, attributes, context);
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
		void ReadField(BinaryReader input, C context, FieldData attribute)
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
			var method = typeof(Serialiser<C>).GetTypeInfo().GetDeclaredMethod(methodName);
			method = method.MakeGenericMethod(attribute.Type);
			if (attribute.Attribute.Type == SerialisationType.Reference)
				attribute.Value = method.Invoke(this, new object[] { input, context });
			else
				attribute.Value = method.Invoke(this, new object[] { input, context, attribute.Value });
		}

		object CallGetFromIdMethod(TypeInfo typeInfo, FieldData[] attributes, C context)
		{
			// Search for a fitting constructor
			foreach (var method in typeInfo.GetDeclaredMethods("GetFromId"))
			{
				if (!method.IsStatic)
					continue;
				List<FieldData> remainingAttributes = new List<FieldData>(attributes);
				var parameters = method.GetParameters();
				object[] arguments = new object[parameters.Length];
				bool success = true;
				for (int i = 0; i < parameters.Length; i++)
				{
					var parameter = parameters[i];
					if (parameter.ParameterType == typeof(C))
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
					return method.Invoke(null, arguments);
			}
			throw new NotImplementedException(typeInfo.Name + " does not specify a static method called 'GetFromId' suitable for deserialisation");
		}
	}
}
