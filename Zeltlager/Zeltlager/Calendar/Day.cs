using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
	}
}
