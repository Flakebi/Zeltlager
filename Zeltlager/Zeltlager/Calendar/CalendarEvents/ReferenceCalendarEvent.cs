using System;
using System.Threading.Tasks;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace Zeltlager.Calendar
{
	// not editable!, serialiseable
	public class ReferenceCalendarEvent : IListCalendarEvent
	{
		StandardCalendarEvent reference;

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Serialisation]
		DateTime date;
		public DateTime Date
		{
			get { return date; }
			set
			{
				date = value.Date.Add(reference.Time);
			}
		}

		bool isShown = true;

		static Task<ReferenceCalendarEvent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult((ReferenceCalendarEvent)context.LagerClient.Calendar.GetEventFromPacketId(id));
		}

		public ReferenceCalendarEvent(PacketId id, StandardCalendarEvent reference)
		{
			this.reference = reference;
			Id = id;
		}

		public CalendarEvent GetEditableCalendarEvent()
		{
			return new ExRefCalendarEvent(Id, Date, reference.Title, reference.Detail, this, reference.GetLager());
		}

		public void makeInvisible()
		{
			isShown = false;
		}

		public int CompareTo(IListCalendarEvent other)
		{
			return Date.CompareTo(other.GetEditableCalendarEvent().Date);
		}
	}
}
