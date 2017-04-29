using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Zeltlager.Calendar
{
	using Client;
	using DataPackets;
	using Zeltlager.Serialisation;

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

		public Day FindClosestDayToNow()
		{
			Day d = Days.Find(day => day.Date.Date >= DateTime.Now.Date);
			if (d == null)
			{
				d = Days.Last();
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

		public void RemoveCalendarEvent(IListCalendarEvent calendarEvent)
		{
			Day d = FindCorrectDay(calendarEvent);
			d.Events.Remove(calendarEvent);
			if (!d.Events.Any())
				Days.Remove(d);
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

		async public void IncludeStandardEvents()
		{
			foreach (Day d in Days)
			{
				foreach (StandardCalendarEvent sce in StandardEvents)
				{
					// check whether the event was added before that day+eventtime & ignore in that case
					if (sce.Id.Packet.Timestamp > d.Date.Add(sce.Time))
					{
						continue;
					}
					// check whether there already is a reference event for that sce on that day
					if (d.Events.Where(ilce => ilce is ReferenceCalendarEvent).Cast<ReferenceCalendarEvent>().Any(rce => rce.Reference.Equals(sce)))
					{
						continue;
					}
					LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
					Serialiser<LagerClientSerialisationContext> serialiser = lager.ClientSerialiser;
					await lager.AddPacket(await AddPacket.Create(serialiser, context, new ReferenceCalendarEvent(null, sce, d.Date)));
				}
			}
		}
		#endregion
	}
}
