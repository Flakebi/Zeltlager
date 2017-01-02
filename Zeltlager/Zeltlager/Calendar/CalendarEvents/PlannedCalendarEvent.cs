using System;
namespace Zeltlager.Calendar
{
	// events that do not yet have a day or time assignedt
	public class PlannedCalendarEvent : Editable<PlannedCalendarEvent>, IComparable<PlannedCalendarEvent>
	{
		public PlannedCalendarEvent()
		{
		}

		public override PlannedCalendarEvent Clone()
		{
			throw new NotImplementedException();
		}

		public int CompareTo(PlannedCalendarEvent other)
		{
			throw new NotImplementedException();
		}

		public bool Equals(PlannedCalendarEvent other)
		{
			throw new NotImplementedException();
		}
	}
}
