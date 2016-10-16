using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
	using Client;
	using UAM;

	[Editable("Termin")]
	public class CalendarEvent : INotifyPropertyChanged, IComparable<CalendarEvent>, IEditable<CalendarEvent>, IEquatable<CalendarEvent>
	{
		/// <summary>
		/// The date of this event.
		/// </summary>
		private DateTime date;
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
		private TimeSpan timeSpan;
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

		private string title;
		[Editable("Titel")]
		public string Title
		{
			get { return title; }
			set { title = value; OnPropertyChanged(nameof(Title)); }
		}

		private string detail;
		[Editable("Beschreibung")]
		public string Detail
		{
			get { return detail; }
			set { detail = value; OnPropertyChanged(nameof(Detail)); }
		}

		public CalendarEvent(DateTime date, string title, string detail)
		{
			this.date = date;
			this.title = title;
			this.detail = detail;
			timeSpan = date.TimeOfDay;
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

		public async Task OnSaveEditing(CalendarEvent oldObj)
		{
			if (oldObj != null)
			{
				// Delete Item
				await LagerClient.CurrentLager.AddPacket(new DeleteCalendarEvent(oldObj));
			}
			// Insert Calendar Event into correct day
			await LagerClient.CurrentLager.AddPacket(new AddCalendarEvent(this));
		}

		public CalendarEvent CloneDeep()
		{
			return new CalendarEvent(date, title, detail);
		}

		public bool Equals(CalendarEvent other)
		{
			return Title.Equals(other.Title) && Detail.Equals(other.Detail) && Date.Equals(other.Date);
		}

		#endregion
	}

	public static class CalendarEventHelper
	{
		public static void Write(this BinaryWriter output, CalendarEvent calendarEvent)
		{
			output.Write(calendarEvent.Date.ToBinary());
			output.Write(calendarEvent.Title);
			output.Write(calendarEvent.Detail);
		}

		public static CalendarEvent ReadCalendarEvent(this BinaryReader input)
		{
			DateTime date = DateTime.FromBinary(input.ReadInt64());
			string title = input.ReadString();
			string detail = input.ReadString();
			return new CalendarEvent(date, title, detail);
		}
	}
}