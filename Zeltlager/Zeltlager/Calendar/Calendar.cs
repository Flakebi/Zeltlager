using System;
using System.Collections.Generic;
using Zeltlager.DataPackets;
using System.Linq;
using System.Collections.ObjectModel;

namespace Zeltlager.Calendar
{
	public class Calendar
	{
		public List<Day> Days { get; }

		public ObservableCollection<CalendarEvent> StandardEvents { get; }
		public ObservableCollection<CalendarEvent> FutureEvents { get; }

		public Calendar()
		{
			Days = new List<Day>();

			// For testing
			//InitCalendar(new DateTime(2016, 8, 2), new DateTime(2016, 8, 12));
		}

		public void InitCalendar(DateTime startDate, DateTime endDate)
		{
			if (endDate < startDate)
			{
				var temp = endDate;
				endDate = startDate;
				startDate = temp;
			}
			Days.Clear();
			while (startDate.Date <= endDate.Date)
			{
				Days.Add(new Day(startDate));
				startDate = startDate.AddDays(1);
			}

			// Add standard events
			foreach (Day day in Days)
			{
				//day.Events.Add(new CalendarEvent(GetSpecificTime(day.Date, 8, 1), "Frühstück", ""));
				//day.Events.Add(new CalendarEvent(GetSpecificTime(day.Date, 12, 30), "Mittagessen", "Maultaschen"));
				//day.Events.Add(new CalendarEvent(GetSpecificTime(day.Date, 18, 30), "Abendessen", "Lagerburger"));
			}
		}

		public static DateTime GetSpecificTime(DateTime day, int newHour, int newMin)
		{
			return new DateTime(day.Year, day.Month, day.Day, newHour, newMin, 0);
		}

		public void InsertNewCalendarEvent(CalendarEvent calendarEvent)
		{
			// Find correct day
			Day d = FindCorrectDay(calendarEvent);
			d.Events.Add(calendarEvent);
			d.Events.Sort();
		}

		public void RemoveCalendarEvent(CalendarEvent caldendarEvent)
		{
			FindCorrectDay(caldendarEvent).Events.Remove(caldendarEvent);
		}

		Day FindCorrectDay(CalendarEvent ce)
		{
			Day d = Days.Find(x => x.Date.Date == ce.Date.Date);
			if (d == null)
			{
				d = new Day(ce.Date.Date);
				Days.Add(d);
			}
			return d;
		}

		public CalendarEvent GetEventFromPacketId(PacketId id)
		{
			return Days.SelectMany(day => day.Events).First(x => x.Id == id);
		}
	}
}
