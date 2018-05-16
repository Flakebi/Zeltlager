using System;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
	using Client;
	using DataPackets;
		using UAM;

	[Editable("Termin")]
	public class CalendarEvent : StandardCalendarEvent, IListCalendarEvent
	{
		[Editable("Tag")]
		public DateTime Date
		{
			get { return date; }
			// make date reflect correct time of day (hate to the DatePicker!!!)
			set
			{
				date = value.Date.Add(Time);
			}
		}

		public CalendarEvent() { }

		public CalendarEvent(PacketId id, DateTime date, string title, string detail, LagerClient lager)
			: base (id, date.TimeOfDay, title, detail, lager)
		{
			this.date = date;
		}

		//// remove and insert again so it is in the correct day
		//public void BeforeEdit(LagerClientSerialisationContext context)
		//{
		//	context.LagerClient.Calendar.RemoveCalendarEvent(this);
		//}

		//public void AfterEdit(LagerClientSerialisationContext context)
		//{
		//	context.LagerClient.Calendar.InsertNewCalendarEvent(this);
		//}

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