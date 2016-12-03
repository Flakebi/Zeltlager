using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
	using Client;
	using DataPackets;
	using Serialisation;
	using UAM;

	[Editable("Termin")]
	public class CalendarEvent : INotifyPropertyChanged, IComparable<CalendarEvent>, IEditable<CalendarEvent>, IEquatable<CalendarEvent>
	{
		public LagerClient Lager { get; set;}

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		/// <summary>
		/// The date of this event.
		/// </summary>
		[Serialisation]
		DateTime date;
		[Editable("Tag")]
		public DateTime Date
		{
			get { return date; }
			// make date reflect correct time of day (hate to the DatePicker!!!)
			set
			{
				date = value.Date.Add(timeSpan);
				OnPropertyChanged(nameof(Date));
				OnPropertyChanged(nameof(TimeSpan));
				OnPropertyChanged(nameof(TimeString));
			}
		}
		/// <summary>
		/// The time of this event, used to edit only the time.
		/// </summary>
		/// A private attribute is needed, so binding Date to a DatePicker does not fuck up our time
		/// (changes in the TimeOfDay in Date are not reflected in TimeSpan)
		TimeSpan timeSpan;
		[Editable("Uhrzeit")]
		public TimeSpan TimeSpan
		{
			get { return timeSpan; }
			set
			{
				date = date.Date.Add(value);
				timeSpan = value;
				OnPropertyChanged(nameof(Date));
				OnPropertyChanged(nameof(TimeSpan));
				OnPropertyChanged(nameof(TimeString));
			}
		}
		/// <summary>
		/// used to display the time of the event nicely
		/// </summary>
		/// <value>The time string.</value>
		public string TimeString
		{
			get { return date.ToString("HH:mm"); }
		}

		string title;
		[Editable("Titel")]
		[Serialisation]
		public string Title
		{
			get { return title; }
			set { title = value; OnPropertyChanged(nameof(Title)); }
		}

		string detail;
		[Editable("Beschreibung")]
		[Serialisation]
		public string Detail
		{
			get { return detail; }
			set { detail = value; OnPropertyChanged(nameof(Detail)); }
		}

		protected static Task<CalendarEvent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.Calendar.GetEventFromPacketId(id));
		}

		public CalendarEvent() {}

		public CalendarEvent(LagerClientSerialisationContext context) : this() {}

		public CalendarEvent(PacketId id, DateTime date, string title, string detail, LagerClient lager)
		{
			this.Id = id;
			this.date = date;
			this.title = title;
			this.detail = detail;
			this.Lager = lager;
			timeSpan = date.TimeOfDay;
		}

		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			Lager = context.LagerClient;
			context.LagerClient.Calendar.InsertNewCalendarEvent(this);
		}

		public void Edit(LagerClientSerialisationContext context)
		{
			// remove and insert again so it is in the correct day
			context.LagerClient.Calendar.RemoveCalendarEvent(this);
			context.LagerClient.Calendar.InsertNewCalendarEvent(this);
		}

		#region Interface implementations

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public int CompareTo(CalendarEvent other)
		{
			return Date.CompareTo(other.Date);
		}

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, CalendarEvent oldObject)
		{
			DataPacket packet;
			if (oldObject != null)
				packet = await EditPacket.Create(serialiser, context, this);
			else
				packet = await AddPacket.Create(serialiser, context, this);
			await context.LagerClient.AddPacket(packet);
		}

		public CalendarEvent Clone()
		{
			return new CalendarEvent(Id?.Clone(), date, title, detail, Lager);
		}

		public bool Equals(CalendarEvent other)
		{
			return Title.Equals(other.Title) && Detail.Equals(other.Detail) && Date.Equals(other.Date);
		}

		#endregion
	}
}