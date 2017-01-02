using System;
namespace Zeltlager.Calendar
{
	// for those shown in the Day-Event Lists
	public interface IListCalendarEvent
	{
		CalendarEvent GetEditableCalendarEvent();
	}
}
