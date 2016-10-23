using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

		bool writeIds;

		public Serialiser(bool writeIds = false)
		{
			this.writeIds = writeIds;
		}

		/// <summary>
		/// Write the given object to the output stream.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be written.</typeparam>
		/// <param name="output">The output stream for the serialisation.</param>
		/// <param name="context">The context for user defined serialisations.</param>
		/// <param name="obj">The object that should be written into output.</param>
		public Task Write<T>(BinaryWriter output, C context, T obj)
		{
			return Write(output, context, obj, typeof(T));
		}

		async Task Write(BinaryWriter output, C context, object obj, Type type)
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			// Check if the object implements ISerialisable
			ISerialisable<C> serialisable = obj as ISerialisable<C>;
			if (serialisable != null)
				await serialisable.Write(output, this, context);
			else if (PRIMITIVES.ContainsKey(type))
			{
				// Check for primitives
				// Search for the matching method
				typeof(BinaryWriter).GetRuntimeMethod(nameof(Write), new Type[] { type })
					.Invoke(output, new object[] { obj });
			} else if (PRIMITIVES.ContainsKey(Nullable.GetUnderlyingType(type)))
			{
				// Save if the object is null
				if (obj == null)
					output.Write(false);
				else
				{
					output.Write(true);
					Type nullableType = Nullable.GetUnderlyingType(type);
					// Write the object
					await Write(output, context, Convert.ChangeType(obj, nullableType), nullableType);
				}
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

				// Serialise all elements
				foreach (var o in list)
					await Write(output, context, o, elementType);
			} else
			{
				// Collect attributes in an array
				var attributes = GetFieldData(type, obj).ToArray();
				foreach (var attribute in attributes)
					await WriteField(output, context, attribute);
			}
		}

		/// <summary>
		/// Write the id of a given object to the output stream.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be written.</typeparam>
		/// <param name="output">The output stream for the serialisation.</param>
		/// <param name="context">The context for user defined serialisations.</param>
		/// <param name="obj">The object of which the id should be written into output.</param>
		public Task WriteId<T>(BinaryWriter output, C context, T obj)
		{
			return WriteId(output, context, obj, typeof(T));
		}

		public async Task WriteId(BinaryWriter output, C context, object obj, Type type)
		{
			// Check if the object implements ISerialisable
			ISerialisable<C> serialisable = obj as ISerialisable<C>;
			if (serialisable != null)
				await serialisable.WriteId(output, this, context);
			else
			{
				// Find the id of the type
				var attributes = GetFieldData(type, obj).Where(a => a.Attribute.Type == SerialisationType.Id).ToArray();
				if (attributes.Length == 0)
				{
					// Check if it's a list of ids
					TypeInfo typeInfo = type.GetTypeInfo();
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

						// Serialise all elements
						foreach (var o in list)
							await WriteId(output, context, o, elementType);
					}
				} else
				{
					foreach (var attribute in attributes)
						await WriteField(output, context, attribute);
				}
			}
		}

		async Task WriteField(BinaryWriter output, C context, FieldData attribute)
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
			if (attribute.Attribute.Type == SerialisationType.Reference)
				await WriteId(output, context, attribute.Value, attribute.Type);
			else
				await Write(output, context, attribute.Value, attribute.Type);
		}

		/// <summary>
		/// Read an object from the given input stream and store the data in the given object.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be read.</typeparam>
		/// <param name="input">The input stream for the serialisation.</param>
		/// <param name="context">The context for user defined serialisations.</param>
		/// <param name="obj">The object that should be filled with the read data.</param>
		/// <returns>The read object.</returns>
		public async Task<T> Read<T>(BinaryReader input, C context, T obj)
		{
			return (T)await Read(input, context, obj, typeof(T));
		}

		async Task<object> Read(BinaryReader input, C context, object obj, Type type)
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			// Check if the object implements ISerialisable
			ISerialisable<C> serialisable = obj as ISerialisable<C>;
			if (serialisable != null)
				await serialisable.Read(input, this, context);
			else if (PRIMITIVES.ContainsKey(type))
			{
				// Check for primitives
				// Search for the matching method
				obj = typeof(BinaryReader).GetRuntimeMethod(nameof(Read) + PRIMITIVES[type], new Type[0])
					.Invoke(input, new object[0]);
			} else if (PRIMITIVES.ContainsKey(Nullable.GetUnderlyingType(type)))
			{
				// Read if the object is null
				if (!input.ReadBoolean())
					obj = GetDefault(type);
				else
				{
					Type nullableType = Nullable.GetUnderlyingType(type);
					// Read the object
					obj = await Read(input, context, Convert.ChangeType(obj, nullableType), nullableType);
				}
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
						obj = constructor.Invoke(new object[] { count });
					else
						obj = constructor.Invoke(new object[0]);
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

				// Read list elements
				for (int i = 0; i < count; i++)
				{
					object element = null;
					if (list.Count > i)
						element = list[i];

					// Read the list element
					element = await Read(input, context, element, elementType);

					if (list.Count > i)
						list[i] = element;
					else
						list.Add(element);
				}
			} else
			{
				// Collect attributes in an array
				var attributes = GetFieldData(type, obj).ToArray();
				// Read all attributes and set the fields of the object
				foreach (var attribute in attributes)
				{
					await ReadField(input, context, attribute);
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
		public async Task<T> ReadFromId<T>(BinaryReader input, C context)
		{
			return (T)await ReadFromId(input, context, typeof(T));
		}

		static async Task<object> GetObjectTask<T>(Task<T> task)
		{
			return await task;
		}

		async Task<object> ReadFromId(BinaryReader input, C context, Type type)
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			// To convert a task
			var converter = typeof(Serialiser<C>).GetTypeInfo()
				.GetDeclaredMethod(nameof(GetObjectTask))
				.MakeGenericMethod(type);
			if (typeInfo.ImplementedInterfaces.Contains(typeof(ISerialisable<C>)))
			{
				// Find a matching method
				var method = typeInfo.GetDeclaredMethods(nameof(ReadFromId))
					.First(m => m.IsStatic && m.GetParameters().Select(
						p => p.ParameterType)
						.SequenceEqual(new Type[] { typeof(BinaryReader), typeof(Serialiser<C>), typeof(C) }));
				object task = method.Invoke(null, new object[] { input, this, context });
				// Convert the task
				return await (Task<object>)converter.Invoke(null, new object[] { task });
			} else
			{
				// Find the id of the type
				var attributes = GetFieldData(type, null).Where(a => a.Attribute.Type == SerialisationType.Id).ToArray();
				if (attributes.Length == 0)
				{
					// Check if it's a list of ids
					if (typeInfo.ImplementedInterfaces.Contains(typeof(IList)))
					{
						object obj;
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
							obj = constructor.Invoke(new object[] { count });
						else
							obj = constructor.Invoke(new object[0]);

						IList list = (IList)obj;
						// Get the type parameter if it is a generic list or the element
						Type elementType = typeof(object);
						if (typeInfo.IsGenericType)
							elementType = typeInfo.GenericTypeArguments[0];
						else if (typeInfo.IsArray)
							elementType = typeInfo.GetElementType();
						else
							elementType = typeof(object);

						// Read list elements
						for (int i = 0; i < count; i++)
						{
							object element = null;
							if (list.Count > i)
								element = list[i];

							// Read the list element
							element = await ReadFromId(input, context, elementType);

							if (list.Count > i)
								list[i] = element;
							else
								list.Add(element);
						}

						return obj;
					}
				} else
				{
					// Read the argumenst to get the id
					foreach (var attribute in attributes)
						await ReadField(input, context, attribute);
				}

				object task = CallGetFromIdMethod(typeInfo, attributes, context);
				// Convert the task
				return await (Task<object>)converter.Invoke(null, new object[] { task });
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
		async Task ReadField(BinaryReader input, C context, FieldData attribute)
		{
			// Check if the attribute is optional
			if (attribute.Attribute.Optional)
			{
				if (!input.ReadBoolean())
				{
					attribute.Value = GetDefault(attribute.Type);
					return;
				}
			}
			// Check if we should read a reference
			if (attribute.Attribute.Type == SerialisationType.Reference)
				attribute.Value = await ReadFromId(input, context, attribute.Type);
			else
				attribute.Value = await Read(input, context, attribute.Value, attribute.Type);
		}

		/// <summary>
		/// Returns a Task<T>.
		/// </summary>
		/// <param name="typeInfo"></param>
		/// <param name="attributes"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		object CallGetFromIdMethod(TypeInfo typeInfo, FieldData[] attributes, C context)
		{
			// Search for a fitting constructor
			foreach (var method in typeInfo.GetDeclaredMethods("GetFromId"))
			{
				var returnType = method.ReturnType.GetTypeInfo();
				// Check the return type and if the method is static
				if (!method.IsStatic ||
					!returnType.IsGenericType ||
					returnType.GetGenericTypeDefinition() != typeof(Task<>))
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
