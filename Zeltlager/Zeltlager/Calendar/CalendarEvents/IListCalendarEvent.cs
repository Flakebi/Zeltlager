using System;
using Zeltlager.DataPackets;

namespace Zeltlager.Calendar
{
	// for those shown in the Day-Event Lists
	public interface IListCalendarEvent : IComparable<IListCalendarEvent>
	{
		CalendarEvent GetEditableCalendarEvent();
		DateTime Date { get; set; }
		PacketId Id { get; set; }

		// for displaying
		string TimeString { get; }
		string Title { get; }
		string Detail { get; }

		bool IsVisible { get; set; }
	}
}
