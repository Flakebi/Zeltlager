using System;
using System.Threading.Tasks;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace Zeltlager.Calendar
{
	// not editable!, serialiseable
	public class ReferenceCalendarEvent : IListCalendarEvent
	{
		[Serialisation(Type = SerialisationType.Reference)]
		public StandardCalendarEvent Reference { get; set; }

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Serialisation]
		DateTime date;
		public DateTime Date
		{
			get { return date; }
			set
			{
				date = value.Date.Add(Reference.Time);
			}
		}

		public string TimeString => Reference.TimeString;
		public string Title => Reference.Title;
		public string Detail => Reference.Detail;

		public bool IsShown { get; private set; }  = true;

		static Task<ReferenceCalendarEvent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult((ReferenceCalendarEvent)context.LagerClient.Calendar.GetEventFromPacketId(id));
		}

		public ReferenceCalendarEvent(PacketId id, StandardCalendarEvent reference)
		{
			Reference = reference;
			Id = id;
		}

		public CalendarEvent GetEditableCalendarEvent()
		{
			return new ExRefCalendarEvent(this);
		}

		public void MakeInvisible()
		{
			IsShown = false;
		}

		public int CompareTo(IListCalendarEvent other)
		{
			return Date.CompareTo(other.GetEditableCalendarEvent().Date);
		}
	}
}
