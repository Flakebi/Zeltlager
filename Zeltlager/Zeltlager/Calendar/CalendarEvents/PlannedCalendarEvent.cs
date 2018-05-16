using System;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
	using Client;
	using DataPackets;
	using Newtonsoft.Json;
		using UAM;

	/// <summary>
	/// Events that do not yet have a day or time assigned.
	/// </summary>
	[Editable("Geplanten Termin")]
	public class PlannedCalendarEvent : Editable<PlannedCalendarEvent>, IComparable<PlannedCalendarEvent>, IDeletable
	{
		protected LagerClient lager;

		[JsonIgnore]
		public PacketId Id { get; set; }

		[Editable("Titel")]
		public string Title { get; set; }

		[Editable("Beschreibung", true)]
		public string Detail { get; set; }

		public bool IsVisible { get; set; } = true;

		public PlannedCalendarEvent() {}

		public PlannedCalendarEvent(PacketId id, string title, string detail, LagerClient lager)
		{
			Id = id;
			Title = title;
			Detail = detail;
			this.lager = lager;
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
