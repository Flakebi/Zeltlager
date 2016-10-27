using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	public class AddPacket : DataPacket
	{
		/// <summary>
		/// The list of types that can be serialised.
		/// Every type must have a constructor that takes
		/// a LagerClientSerialisationContext. This constructor
		/// should also reserve the id for the newly created object.
		/// 
		/// The method is an optional member method that will
		/// be called on the newly created type after if was read
		/// and when it should be added to the lager. This method
		/// can also take a LagerClientSerialisationContext.
		/// </summary>
		static readonly Tuple<Type, MethodInfo>[] types = {
			new Tuple<Type, MethodInfo>(typeof(Member), typeof(Member).GetRuntimeMethod("Add", new Type[] { typeof(LagerClientSerialisationContext) })),
		};

		public static byte GetIdCount() { return (byte)types.Length; }

		public AddPacket() { }

		public async Task Init(LagerClientSerialisationContext context, object obj)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				// Get the generic method
				var method = typeof(Serialiser<LagerClientSerialisationContext>).GetTypeInfo().GetDeclaredMethod(nameof(Serialiser<LagerClientSerialisationContext>.Write));
				// Find the object type to write
				Type objectType = obj.GetType();
				bool success = false;
				for (int i = 0; i < types.Length; i++)
				{
					if (types[i].Item1 == objectType)
					{
						// Set id and write object
						Id = (byte)i;

						method = method.MakeGenericMethod(types[i].Item1);
						await (Task)method.Invoke(context.Lager.Serialiser, new object[] { output, context, obj });
						success = true;
						break;
					}
				}
				if (!success)
					throw new ArgumentException("The object is not supported for adding, add it to the types array in AddPacket");
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise(LagerClientSerialisationContext context)
		{
			var type = types[Id];
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				// Create an object
				object obj = type.Item1.GetTypeInfo().DeclaredConstructors
					.First(c => c.GetParameters().Select(p => p.ParameterType)
					.SequenceEqual(new Type[] { typeof(LagerClientSerialisationContext) }))
					.Invoke(new object[] { context });

				// Get the generic method
				var method = typeof(Serialiser<LagerClientSerialisationContext>).GetTypeInfo().GetDeclaredMethod(nameof(Serialiser<LagerClientSerialisationContext>.Read));
				method = method.MakeGenericMethod(type.Item1);
				object task = method.Invoke(context.Lager.Serialiser, new object[] { output, context, obj });
				// To convert a task
				var converter = typeof(Serialiser<LagerClientSerialisationContext>).GetTypeInfo()
					.GetDeclaredMethod(nameof(Serialiser<LagerClientSerialisationContext>.GetObjectTask))
					.MakeGenericMethod(type.Item1);
				// Wait for the task
				await (Task<object>)converter.Invoke(null, new object[] { task });

				// Call the add method
				if (type.Item2 != null)
				{
					object[] parameters;
					if (type.Item2.GetParameters().Select(p => p.ParameterType).SequenceEqual(new Type[] { typeof(LagerClientSerialisationContext) }))
						parameters = new object[] { context };
					else
						parameters = new object[0];
					object result = type.Item2.Invoke(obj, parameters);
					// Wait if its a task
					if (type.Item2.ReturnType == typeof(Task))
						await (Task)result;
				}
			}
		}
	}
}
