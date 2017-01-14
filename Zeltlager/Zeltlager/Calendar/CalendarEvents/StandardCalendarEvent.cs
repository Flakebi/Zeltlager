using System;
using Zeltlager.UAM;
using Zeltlager.Serialisation;
using Zeltlager.DataPackets;
using Zeltlager.Client;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
	// events that occour on multiple days at the same time
	[Editable("regelmäßigen Termin")]
	public class StandardCalendarEvent : PlannedCalendarEvent, IComparable<StandardCalendarEvent>
	{
		/// <summary>
		/// The time of this event, used to edit only the time.
		/// </summary>
		[Editable("Uhrzeit")]
		public virtual TimeSpan Time { get; set; }

		public string TimeString
		{
			get { return Time.ToString("hh':'mm"); }
		}

		public StandardCalendarEvent() {}

		public StandardCalendarEvent(LagerClientSerialisationContext context) : this() {}

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
			return new StandardCalendarEvent(Id, Time, Title, Detail, lager);
		}

		public int CompareTo(StandardCalendarEvent other)
		{
			return Time.CompareTo(Time);
		}
	}
}
