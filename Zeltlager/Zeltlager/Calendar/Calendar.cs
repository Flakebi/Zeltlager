using System;
using System.Collections.Generic;
using Zeltlager.DataPackets;
using System.Linq;
using System.Collections.ObjectModel;
using Zeltlager.Client;

namespace Zeltlager.Calendar
{
	public class Calendar
	{
		LagerClient lager;

		public List<Day> Days { get; } = new List<Day>();

		public ObservableCollection<StandardCalendarEvent> StandardEvents { get; } 
			= new ObservableCollection<StandardCalendarEvent>();
		public ObservableCollection<PlannedCalendarEvent> PlannedEvents { get; } 
			= new ObservableCollection<PlannedCalendarEvent>();

		public Calendar(LagerClient lager)
		{
			this.lager = lager;
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

		public LagerClient GetLager()
		{
			return lager;
		}

		Day FindCorrectDay(IListCalendarEvent ce)
		{
			Day d = Days.Find(x => x.Date.Date == ce.Date.Date);
			if (d == null)
			{
				d = new Day(ce.Date.Date);
				Days.Add(d);
				Days.Sort();
			}
			return d;
		}

		#region CalendarEvents
		public void InsertNewCalendarEvent(IListCalendarEvent calendarEvent)
		{
			// Find correct day
			Day d = FindCorrectDay(calendarEvent);
			d.Events.Add(calendarEvent);
			d.Events.Sort();
		}

		public void RemoveCalendarEvent(IListCalendarEvent caldendarEvent)
		{
			FindCorrectDay(caldendarEvent).Events.Remove(caldendarEvent);
		}

		public IListCalendarEvent GetEventFromPacketId(PacketId id)
		{
			return Days.SelectMany(day => day.Events).First(x => x.Id == id);
		}
		#endregion CalendarEvents

		#region PlannedCEs
		public void InsertNewPlannedCalendarEvent(PlannedCalendarEvent pce)
		{
			PlannedEvents.Add(pce);
			PlannedEvents.Sort();
		}

		public void RemovePlannedCalendarEvent(PlannedCalendarEvent pce)
		{
			PlannedEvents.Remove(pce);
		}

		public PlannedCalendarEvent GetPlannedEventFromPacketId(PacketId id)
		{
			return PlannedEvents.First(x => x.Id == id);
		}
		#endregion

		#region StandardCEs
		public void InsertNewStandardCalendarEvent(StandardCalendarEvent sce)
		{
			StandardEvents.Add(sce);
		}

		public void RemoveStandardCalendarEvent(StandardCalendarEvent sce)
		{
			StandardEvents.Remove(sce);
		}

		public StandardCalendarEvent GetStandardEventFromPacketId(PacketId id)
		{
			return StandardEvents.First(x => x.Id == id);
		}

		public void IncludeStandardEvents()
		{
			foreach (Day d in Days)
			{
				foreach (StandardCalendarEvent sce in StandardEvents)
				{
					// check whether the event was added before that day+eventtime & ignore in that case
					if (sce.Id.Bundle.Packets[sce.Id.PacketIndex.Value].Timestamp > d.Date.Add(sce.Time))
					{
						continue;
					}
					// check whether there already is a reference event for that sce on that day
					if (d.Events.Where(ilce => ilce is ReferenceCalendarEvent).Cast<ReferenceCalendarEvent>().Any(rce => rce.Reference.Equals(sce)))
					{
						continue;
					}
					d.Events.Add(new ReferenceCalendarEvent(null, sce));
				}
			}
		}
		#endregion
	}
}
