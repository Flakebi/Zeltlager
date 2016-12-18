using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Zeltlager.Calendar;
using Zeltlager.Competition;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	public class AddPacket : DataPacket
	{
		/// <summary>
		/// The list of types that can be serialised.
		/// Every type must have a constructor that takes
		/// a LagerClientSerialisationContext.
		/// 
		/// The method is an optional member method that will
		/// be called on the newly created object after it was read
		/// and when it should be added to the lager. This method
		/// can also take a LagerClientSerialisationContext.
		/// </summary>
		static readonly Tuple<Type, MethodInfo>[] types = {
			new Tuple<Type, MethodInfo>(typeof(Member),
				typeof(Member).GetRuntimeMethod("Add", new Type[] { typeof(LagerClientSerialisationContext) })),
			new Tuple<Type, MethodInfo>(typeof(Tent),
				typeof(Tent).GetRuntimeMethod("Add", new Type[] { typeof(LagerClientSerialisationContext) })),
			new Tuple<Type, MethodInfo>(typeof(CalendarEvent),
			    typeof(CalendarEvent).GetRuntimeMethod("Add", new Type[] { typeof(LagerClientSerialisationContext) })),
			new Tuple<Type, MethodInfo>(typeof(Competition.Competition),
			    typeof(Competition.Competition).GetRuntimeMethod("Add", new Type[] { typeof(LagerClientSerialisationContext) })),
			new Tuple<Type, MethodInfo>(typeof(Station),
			    typeof(Station).GetRuntimeMethod("Add", new Type[] { typeof(LagerClientSerialisationContext) })),
			new Tuple<Type, MethodInfo>(typeof(CompetitionResult),
			    typeof(CompetitionResult).GetRuntimeMethod("Add", new Type[] { typeof(LagerClientSerialisationContext) })),
		};

		public static int GetIdCount() { return types.Length; }

		protected AddPacket() { }

        public static async Task<AddPacket> Create(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, object obj)
		{
			var packet = new AddPacket();
			await packet.Init(serialiser, context, obj);
			return packet;
		}

		async Task Init(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, object obj)
		{
			var mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				// Find the object type to write
				Type objectType = obj.GetType();
				bool success = false;
				for (int i = 0; i < types.Length; i++)
				{
					if (types[i].Item1 == objectType)
					{
						// Set id and write object
						subId = i;

						await serialiser.Write(output, context, obj, types[i].Item1);
						success = true;
						break;
					}
				}
				if (!success)
					throw new ArgumentException("The object is not supported for adding, add it to the types array in AddPacket");
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			var type = types[subId];
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
			{
				// Create an object
				object obj = type.Item1.GetTypeInfo().DeclaredConstructors
					.First(c => c.GetParameters().Select(p => p.ParameterType)
						.SequenceEqual(new Type[] { typeof(LagerClientSerialisationContext) }))
					.Invoke(new object[] { context });

				obj = await serialiser.Read(input, context, obj, type.Item1);

				// Call the add method
				if (type.Item2 != null)
				{
					object[] parameters;
					if (type.Item2.GetParameters().Select(p => p.ParameterType)
					    .SequenceEqual(new Type[] { typeof(LagerClientSerialisationContext) }))
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
