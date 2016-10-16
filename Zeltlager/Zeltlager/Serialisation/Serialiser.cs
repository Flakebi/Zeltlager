using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zeltlager.Serialisation
{
	public class Serialiser
	{
		struct FieldData
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

		static readonly Type[] PRIMITIVES = {
			typeof(string),
			typeof(bool),
			// Numbers
			typeof(byte),
			typeof(sbyte),
			typeof(ushort),
			typeof(short),
			typeof(uint),
			typeof(int),
			typeof(ulong),
			typeof(long),
		};

		public void Write<T>(BinaryWriter output, SerialisationContext context, T obj)
		{
			var type = typeof(T);
			TypeInfo typeInfo = type.GetTypeInfo();
			// Check if the object implements ISerialisable
			ISerialisable serialisable = obj as ISerialisable;
			if (serialisable != null)
				serialisable.Write(output, context);
			else if (PRIMITIVES.Contains(type))
			{
				// Check for primitives
				// Search for the matching method
				typeof(BinaryWriter).GetTypeInfo().GetDeclaredMethods(nameof(Write)).First(m =>
				{
					var arguments = m.GetParameters();
					return arguments.Length == 1 && arguments[0].ParameterType == typeof(T);
				}).Invoke(output, new object[] { obj });
			} else if (type is IList)
			{
				// Check for lists
				var list = (IList)obj;
				output.Write((ushort)list.Count);
				foreach (var o in list)
				{
					var method = typeof(Serialiser).GetTypeInfo().GetDeclaredMethod(nameof(Write));
					// Get the type parameter if it is a generic list
					method = method.MakeGenericMethod(typeInfo.IsGenericType ? typeInfo.GenericTypeParameters[0] : typeof(object));
					method.Invoke(this, new object[] { output, context, o });
				}
			} else
			{
				// Get a tuple of name, type and value
				var fields = typeInfo.DeclaredFields
					.Where(f => f.GetCustomAttribute<SerialisationAttribute>() != null)
					.Select(f => new FieldData(f.Name, f.FieldType, f.GetValue(obj), f.GetCustomAttribute<SerialisationAttribute>()));
				var properties = typeInfo.DeclaredProperties
					.Where(p => p.GetCustomAttribute<SerialisationAttribute>() != null)
					.Select(p => new FieldData(p.Name, p.PropertyType, p.GetValue(obj), p.GetCustomAttribute<SerialisationAttribute>()));
				// Sort by name and collect in an array
				var attributes = fields.Concat(properties).OrderBy(t => t.Name).ToArray();
				foreach (var attribute in attributes)
				{
					// Check if we should only save a reference
					string methodName = attribute.Attribute.Type == SerialisationType.Reference ? nameof(WriteId) : nameof(Write);
					// Use the previously taken type as the generic parameter for the method call
					var method = typeof(Serialiser).GetTypeInfo().GetDeclaredMethod(methodName);
					method = method.MakeGenericMethod(attribute.Type);
					method.Invoke(this, new object[] { output, context, attribute.Value });
				}
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
				//TODO
				// Find the id of the type
			}
		}

		public T Read<T>(BinaryReader input, SerialisationContext context)
		{
			// Check if the object implements ISerialisable
			TypeInfo typeInfo = typeof(T).GetTypeInfo();
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
			} else
			{
				//TODO
				return default(T);
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
				//TODO
				return default(T);
			}
		}
	}
}
