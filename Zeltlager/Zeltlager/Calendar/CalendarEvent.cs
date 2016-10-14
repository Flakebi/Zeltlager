using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Zeltlager.UAM;
using System.IO;

namespace Zeltlager.Calendar
{
	using System.Threading.Tasks;
	using Client;

	[Editable("Termin")]
	public class CalendarEvent : INotifyPropertyChanged, IComparable<CalendarEvent>, IEditable<CalendarEvent>, IEquatable<CalendarEvent>
	{
		/// <summary>
		/// date of the event
		/// </summary>
		private DateTime date;
		[Editable("Tag")]
		public DateTime Date
		{
			get { return date; }
			// make date reflect correct time of day (hate to the DatePicker!!!)
			set { date = value.Date.Add(timeSpan); OnPropertyChanged("Date"); OnPropertyChanged("TimeSpan"); OnPropertyChanged("TimeString");}
		}
		/// <summary>
		/// time of the event, used to edit only time
		/// </summary>
		/// private attribute needed, so binding Date to a DatePicker does not fuck up our time
		/// (changes in the TimeOfDay in Date are not reflected in TimeSpan)
		private TimeSpan timeSpan;
		[Editable("Uhrzeit")]
		public TimeSpan TimeSpan
		{
			get { return timeSpan; }
			set
			{
				date = date.Date.Add(value); timeSpan = value;
				OnPropertyChanged("TimeSpan"); OnPropertyChanged("TimeString"); OnPropertyChanged("Date");
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
			set { title = value; OnPropertyChanged("Title"); }
		}
		private string detail;
		[Editable("Beschreibung")]
		public string Detail
		{
			get { return detail; }
			set { detail = value; OnPropertyChanged("Detail"); }
		}

		private CalendarEvent(DateTime date, string title)
		{
			this.date = date;
			this.title = title;
			this.timeSpan = date.TimeOfDay;
		}

		public CalendarEvent(DateTime date, string title, string detail) : this(date, title)
		{
			this.detail = detail;
		}

		#region Interface implementations

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName]string propertyName = null)
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
				//Delete Item
				await LagerClient.CurrentLager.AddPacket(new DeleteCalendarEvent(oldObj));
			}
			//Insert Calendar Event into correct day
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