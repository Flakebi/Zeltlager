using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Zeltlager.Calendar;
using Zeltlager.Competition;

namespace Zeltlager.DataPackets
{
	
	public class EditPacket : DataPacket
	{
		/// <summary>
		/// The list of types that can be serialised.
		///
		/// The methods are optional member methods that will
		/// be called on the edited object before and after it was read/modified.
		/// This methods can also take a LagerClientSerialisationContext.
		/// </summary>
		static readonly Tuple<Type, MethodInfo, MethodInfo>[] types = {
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(Member), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(Tent), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(CalendarEvent), 
			   typeof(CalendarEvent).GetRuntimeMethod("BeforeEdit", new Type[] { typeof(LagerClientSerialisationContext) }),
			   typeof(CalendarEvent).GetRuntimeMethod("AfterEdit", new Type[] { typeof(LagerClientSerialisationContext) })),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(Competition.Competition), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(Station), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(CompetitionResult), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(GroupParticipant), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(MemberParticipant), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(TentParticipant), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(PlannedCalendarEvent), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(StandardCalendarEvent), null, null),
			new Tuple<Type, MethodInfo, MethodInfo>(typeof(ReferenceCalendarEvent), null, null),
		};

		public static int GetIdCount() { return types.Length; }

		protected EditPacket() { }

		public static async Task<EditPacket> Create(object obj)
		{
			var packet = new EditPacket();
			await packet.Init(obj);
			return packet;
		}

		async Task Init(object obj)
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
					throw new InvalidOperationException("The object is not supported for editing, add it to the types array in EditPacket");
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise()
		{
			var type = types[subId];
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
			{
				// Get the object by id
				object obj = await serialiser.ReadFromId(input, context, type.Item1);

				// Call the before edit method
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

				obj = await serialiser.Read(input, context, obj, type.Item1);

				// Call the after edit method
				if (type.Item3 != null)
				{
					object[] parameters;
					if (type.Item3.GetParameters().Select(p => p.ParameterType)
						.SequenceEqual(new Type[] { typeof(LagerClientSerialisationContext) }))
						parameters = new object[] { context };
					else
						parameters = new object[0];
					object result = type.Item3.Invoke(obj, parameters);
					// Wait if its a task
					if (type.Item3.ReturnType == typeof(Task))
						await (Task)result;
				}

				contentString = obj.ToString();
			}
		}
	}
}
