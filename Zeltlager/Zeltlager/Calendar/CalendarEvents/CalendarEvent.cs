using System;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
	using Client;
	using DataPackets;
	using Serialisation;
	using UAM;

	[Editable("Termin")]
	public class CalendarEvent : StandardCalendarEvent, IListCalendarEvent
	{
		/// <summary>
		/// The date of this event.
		/// </summary>
		protected DateTime date;
		[Editable("Tag")]
		[Serialisation]
		public DateTime Date
		{
			get { return date; }
			// make date reflect correct time of day (hate to the DatePicker!!!)
			set
			{
				date = value.Date.Add(Time);
			}
		}

		/// A private attribute is needed, so binding Date to a DatePicker does not fuck up our time
		/// (changes in the TimeOfDay in Date are not reflected in TimeSpan)
		[Editable("Uhrzeit")]
		public new TimeSpan Time
		{
			get { return Date.TimeOfDay; }
			set { Date = Date.Date.Add(value); }
		}

		public new string TimeString
		{
			get { return Date.ToString("HH:mm"); }
		}

		static Task<CalendarEvent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult((CalendarEvent)context.LagerClient.Calendar.GetEventFromPacketId(id));
		}

		public CalendarEvent() {}

		public CalendarEvent(LagerClientSerialisationContext context) : this() {}

		public CalendarEvent(PacketId id, DateTime date, string title, string detail, LagerClient lager)
			: base (id, date.TimeOfDay, title, detail, lager)
		{
			Date = date;
		}

		public new void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			context.LagerClient.Calendar.InsertNewCalendarEvent(this);
		}

		public void Edit(LagerClientSerialisationContext context)
		{
			// remove and insert again so it is in the correct day
			context.LagerClient.Calendar.RemoveCalendarEvent(this);
			context.LagerClient.Calendar.InsertNewCalendarEvent(this);
		}

		#region Interface implementations

		public int CompareTo(CalendarEvent other)
		{
			return Date.CompareTo(other.Date);
		}

		public override PlannedCalendarEvent Clone()
		{
			return new CalendarEvent(Id?.Clone(), date, Title, Detail, lager);
		}

		public CalendarEvent GetEditableCalendarEvent()
		{
			return this;
		}

		public int CompareTo(IListCalendarEvent other)
		{
			return CompareTo(other.GetEditableCalendarEvent());
		}

		#endregion
	}
}