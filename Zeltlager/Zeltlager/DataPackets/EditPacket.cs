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

	public class EditPacket : DataPacket
	{
		/// <summary>
		/// The list of types that can be serialised.
		/// 
		/// The method is an optional member method that will
		/// be called on the edited object after it was read.
		/// This method can also take a LagerClientSerialisationContext.
		/// </summary>
		static readonly Tuple<Type, MethodInfo>[] types = {
			new Tuple<Type, MethodInfo>(typeof(Member), null),
			new Tuple<Type, MethodInfo>(typeof(Tent), null),
			new Tuple<Type, MethodInfo>(typeof(CalendarEvent), 
			   typeof(CalendarEvent).GetRuntimeMethod("Edit", new Type[] { typeof(LagerClientSerialisationContext) })),
			new Tuple<Type, MethodInfo>(typeof(Competition.Competition), null),
			new Tuple<Type, MethodInfo>(typeof(Station), null),
			new Tuple<Type, MethodInfo>(typeof(CompetitionResult), null),
			new Tuple<Type, MethodInfo>(typeof(GroupParticipant), null),
			new Tuple<Type, MethodInfo>(typeof(MemberParticipant), null),
			new Tuple<Type, MethodInfo>(typeof(TentParticipant), null),
		};

		public static int GetIdCount() { return types.Length; }

		protected EditPacket() { }

        public static async Task<EditPacket> Create(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, object obj)
		{
			var packet = new EditPacket();
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

						await serialiser.WriteId(output, context, obj, types[i].Item1);
						await serialiser.Write(output, context, obj, types[i].Item1);
						success = true;
						break;
					}
				}
				if (!success)
					throw new ArgumentException("The object is not supported for editing, add it to the types array in EditPacket");
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			var type = types[subId];
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
			{
				// Get the object by id
				object obj = await serialiser.ReadFromId(input, context, type.Item1);

				obj = await serialiser.Read(input, context, obj, type.Item1);

				// Call the edit method
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
