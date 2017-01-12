using System;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using Zeltlager.UAM;

namespace Zeltlager.Calendar
{
	// events that do not yet have a day or time assignedt
	public class PlannedCalendarEvent : Editable<PlannedCalendarEvent>, IComparable<PlannedCalendarEvent>
	{
		protected LagerClient lager;

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Titel")]
		[Serialisation]
		public string Title { get; set; }

		[Editable("Beschreibung")]
		[Serialisation]
		public string Detail { get; set; }

		public PlannedCalendarEvent() {}

		public PlannedCalendarEvent(LagerClientSerialisationContext context) : this() {}

		public PlannedCalendarEvent(PacketId id, string title, string detail, LagerClient lager)
		{
			Id = id;
			Title = title;
			Detail = detail;
			this.lager = lager;
		}

		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			context.LagerClient.Calendar.InsertNewPlannedCalendarEvent(this);
		}

		public override PlannedCalendarEvent Clone()
		{
			return new PlannedCalendarEvent(Id, Title, Detail, lager);
		}

		public int CompareTo(PlannedCalendarEvent other)
		{
			int t = Title.CompareTo(other.Title);
			if (t == 0)
				t = Detail.CompareTo(other.Detail);
			return t;
		}

		public bool Equals(PlannedCalendarEvent other)
		{
			return Id.Equals(other.Id);
		}

		public LagerClient GetLager()
		{
			return lager;
		}
	}
}
