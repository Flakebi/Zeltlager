using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using System.Net.Http.Headers;

namespace Zeltlager.Calendar
{
	public class Day : IComparable<Day>
	{
		public Tent Dishwashers { get; set; }

		public ObservableCollection<IListCalendarEvent> Events { get; set; }

		public DateTime Date { get; set; }

		public Day(DateTime date)
		{
			Date = date;
			Events = new ObservableCollection<IListCalendarEvent>();
		}

		public int CompareTo(Day other)
		{
			return Date.CompareTo(other.Date);
		}

		public async Task CreateDishwasherPacket(Tent dishwashers, LagerClient lager)
		{
			Serialiser<LagerClientSerialisationContext> serializer = lager.ClientSerialiser;
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
			DataPacket packet = await DishwasherPacket.Create(serializer, context, Date, dishwashers);
			await lager.AddPacket(packet);
		}
	}
}
