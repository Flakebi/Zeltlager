using System;
using System.IO;
using System.Threading.Tasks;
using Zeltlager.Calendar;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Zeltlager.DataPackets
{
	public class DishwasherPacket : DataPacket
	{
		public DateTime Date { get; private set; }

		public Tent Dishwashers { get; private set; }

		protected DishwasherPacket() { }

		public static async Task<DishwasherPacket> Create(DateTime date, Tent dishwashers)
		{
			var packet = new DishwasherPacket
			{
				Date = date,
				Dishwashers = dishwashers,
			};
			return packet;
		}

		public override async Task Deserialise()
		{

			if (Dishwashers != null)
			{
				Tent dishwashers = await serialiser.ReadFromId<Tent>(input, context);
				day.Dishwashers = dishwashers;
			}
			else
			{
				day.Dishwashers = null;
			}
			// todo wofür ist das hier?
			contentString = "";
		}
	}
}
