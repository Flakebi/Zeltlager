using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zeltlager.DataPackets;
namespace Zeltlager.Calendar
{
	/// <summary>
	/// References a StandardCalendarEvent in the actual Calendar (so it gets a Day, too).
	/// Is serialized, but can not be edited! ExRefCalendarEvents are edited instead.
	/// </summary>
	public class ReferenceCalendarEvent : IListCalendarEvent
	{
		// todo json ref
		public StandardCalendarEvent Reference { get; set; }

		[JsonIgnore]
		public PacketId Id { get; set; }

		DateTime date;
		public DateTime Date
		{
			get { return date; }
			set
			{
				date = value.Date.Add(Reference.Time);
			}
		}

		[JsonIgnore] 
		public string TimeString => Reference.TimeString;

		[JsonIgnore] 
		public string Title => Reference.Title;

		[JsonIgnore] 
		public string Detail => Reference.Detail;

		public bool IsVisible { get; set; } = true;

		public ReferenceCalendarEvent(PacketId id, StandardCalendarEvent reference, DateTime date)
		{
			Reference = reference;
			Id = id;
			Date = date;
		}

		public CalendarEvent GetEditableCalendarEvent()
		{
			return new ExRefCalendarEvent(this);
		}

		public int CompareTo(IListCalendarEvent other)
		{
			return Date.CompareTo(other.GetEditableCalendarEvent().Date);
		}
	}
}
