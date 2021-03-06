using System;
using System.Threading.Tasks;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace Zeltlager.Calendar
{
	/// <summary>
	/// References a StandardCalendarEvent in the actual Calendar (so it gets a Day, too).
	/// Is serialized, but can not be edited! ExRefCalendarEvents are edited instead.
	/// </summary>
	public class ReferenceCalendarEvent : IListCalendarEvent
	{
		[Serialisation(Type = SerialisationType.Reference)]
		public StandardCalendarEvent Reference { get; set; }

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Serialisation]
		DateTime date;
		public DateTime Date
		{
			get { return date; }
			set
			{
				date = value.Date.Add(Reference.Time);
			}
		}

		public string TimeString => Reference.TimeString;
		public string Title => Reference.Title;
		public string Detail => Reference.Detail;

		[Serialisation]
		public bool IsVisible { get; set; } = true;

		static Task<ReferenceCalendarEvent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult((ReferenceCalendarEvent)context.LagerClient.Calendar.GetEventFromPacketId(id));
		}

		public ReferenceCalendarEvent(LagerClientSerialisationContext context) {}

		public ReferenceCalendarEvent(PacketId id, StandardCalendarEvent reference, DateTime date)
		{
			Reference = reference;
			Id = id;
			Date = date;
		}

		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			context.LagerClient.Calendar.InsertNewCalendarEvent(this);
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
