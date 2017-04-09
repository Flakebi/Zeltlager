using System;
using System.Threading.Tasks;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using Zeltlager.UAM;

namespace Zeltlager.Calendar
{
	// events that do not yet have a day or time assignedt
	[Editable("Geplanten Termin")]
	public class PlannedCalendarEvent : Editable<PlannedCalendarEvent>, IComparable<PlannedCalendarEvent>, IDeletable
	{
		protected LagerClient lager;

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Titel")]
		[Serialisation]
		public string Title { get; set; }

		[Editable("Beschreibung",true)]
		[Serialisation]
		public string Detail { get; set; }

		[Serialisation]
		public bool IsVisible { get; set; } = true;

		public PlannedCalendarEvent() {}

		public PlannedCalendarEvent(LagerClientSerialisationContext context) : this() {}

		public PlannedCalendarEvent(PacketId id, string title, string detail, LagerClient lager)
		{
			Id = id;
			Title = title;
			Detail = detail;
			this.lager = lager;
		}

		static Task<PlannedCalendarEvent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.Calendar.GetPlannedEventFromPacketId(id));
		}

		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			context.LagerClient.Calendar.InsertNewPlannedCalendarEvent(this);
		}

		public override PlannedCalendarEvent Clone()
		{
			return new PlannedCalendarEvent(Id?.Clone(), Title, Detail, lager);
		}

		public int CompareTo(PlannedCalendarEvent other)
		{
			int t = string.Compare(Title, other.Title, StringComparison.Ordinal);
			if (t == 0)
				t = string.Compare(Detail, other.Detail, StringComparison.Ordinal);
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
