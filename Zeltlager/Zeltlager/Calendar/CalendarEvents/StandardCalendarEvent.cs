using System;
namespace Zeltlager.Calendar
{
	// events that occour on multiple days at the same time
	public class StandardCalendarEvent : PlannedCalendarEvent, IComparable<StandardCalendarEvent>
	{
		public StandardCalendarEvent()
		{
		}

		public override PlannedCalendarEvent Clone()
		{
			throw new NotImplementedException();
		}

		public int CompareTo(StandardCalendarEvent other)
		{
			throw new NotImplementedException();
		}
	}
}
