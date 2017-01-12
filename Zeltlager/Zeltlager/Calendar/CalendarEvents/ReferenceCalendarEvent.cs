using System;
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

		public ReferenceCalendarEvent(PacketId id, StandardCalendarEvent reference)
		{
			this.reference = reference;
		}

		public CalendarEvent GetEditableCalendarEvent()
		{
			throw new NotImplementedException();
		}
	}
}
