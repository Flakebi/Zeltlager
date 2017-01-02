using System;
namespace Zeltlager.Calendar
{
	// not editable!, serialiseable
	public class ReferenceCalendarEvent : IListCalendarEvent
	{
		public ReferenceCalendarEvent()
		{
		}

		public CalendarEvent GetEditableCalendarEvent()
		{
			throw new NotImplementedException();
		}
	}
}
