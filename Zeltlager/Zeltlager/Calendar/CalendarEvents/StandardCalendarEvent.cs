using System;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
	using Client;
	using DataPackets;
	using Serialisation;
	using UAM;
	
	/// <summary>
	/// events that occour on multiple days at the same time 
	/// </summary>
	[Editable("Regelmäßigen Termin")]
	public class StandardCalendarEvent : PlannedCalendarEvent, IComparable<StandardCalendarEvent>, IEquatable<StandardCalendarEvent>, IDeletable
	{
		/// <summary>
		/// The date of this event.
		/// The StandardCalendarEvent only uses the time of this date
		/// but we save the whole DateTime here, because there is no date-only
		/// class in C# and the CalendarEvent needs both.
		/// </summary>
		[Serialisation]
		protected DateTime date;

		/// <summary>
		/// The time of this event.
		/// </summary>
		[Editable("Uhrzeit")]
		public TimeSpan Time
		{
			get
			{
				return date.TimeOfDay;
			}

			set
			{
				date = date.Date.Add(value);
			}
		}

		public string TimeString => date.ToString("HH:mm");

		public StandardCalendarEvent() { }

		public StandardCalendarEvent(LagerClientSerialisationContext context) : this() { }

		public StandardCalendarEvent(PacketId id, TimeSpan time, string title, string detail, LagerClient lager)
			: base(id, title, detail, lager)
		{
			Time = time;
		}

		static Task<StandardCalendarEvent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.Calendar.GetStandardEventFromPacketId(id));
		}

		public new void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			context.LagerClient.Calendar.InsertNewStandardCalendarEvent(this);
		}

		public override PlannedCalendarEvent Clone()
		{
			return new StandardCalendarEvent(Id?.Clone(), Time, Title, Detail, lager);
		}

		public int CompareTo(StandardCalendarEvent other)
		{
			return Time.CompareTo(other.Time);
		}

		public bool Equals(StandardCalendarEvent other)
		{
			return Time.Equals(other.Time) && Title.Equals(other.Title) && Detail.Equals(other.Detail);
		}
	}
}
