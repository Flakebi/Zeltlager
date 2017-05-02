using System;
using System.IO;
using System.Threading.Tasks;
using Zeltlager.Serialisation;
using Zeltlager.Calendar;

namespace Zeltlager.DataPackets
{
	public class DishwasherPacket : DataPacket
	{
		protected DishwasherPacket() { }

		public static async Task<DishwasherPacket> Create(Serialiser<LagerClientSerialisationContext> serialiser,
												LagerClientSerialisationContext context, DateTime date, Tent dishwashers)
		{
			var packet = new DishwasherPacket();
			await packet.Init(serialiser, context, date, dishwashers);
			return packet;
		}

		async Task Init(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context,  DateTime date, Tent dishwashers)
		{
			var mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				await serialiser.Write(output, context, date);
				if (dishwashers != null)
				{
					await serialiser.Write(output, context, true);
					await serialiser.WriteId(output, context, dishwashers);
				}
				else
				{
					await serialiser.Write(output, context, false);
				}
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
			{
				DateTime date = await serialiser.Read(input, context, new DateTime());
				Day day = context.LagerClient.Calendar.FindCorrectDay(new CalendarEvent { Date = date, });
				bool hasDishwashers = await serialiser.Read(input, context, false);
				if (day != null)
				{
					if (hasDishwashers)
					{
						Tent dishwashers = await serialiser.ReadFromId<Tent>(input, context);
						day.Dishwashers = dishwashers;
					}
					else
					{
						day.Dishwashers = null;
					}
				}
				contentString = "";
			}
		}
	}
}
