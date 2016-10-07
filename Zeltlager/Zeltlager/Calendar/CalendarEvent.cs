using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zeltlager.Calendar
{
	using Client;

	[Editable("Termin")]
	public class CalendarEvent : INotifyPropertyChanged, IComparable<CalendarEvent>, IEditable<CalendarEvent>
	{
		/// <summary>
		/// date of the event
		/// </summary>
		private DateTime date;
		[Editable("Tag")]
		public DateTime Date
		{
			get { return date; }
			set { date = value; OnPropertyChanged("Date"); OnPropertyChanged("TimeSpan"); OnPropertyChanged("TimeString");}
		}
		/// <summary>
		/// time of the event, used to edit only time
		/// </summary>
		private TimeSpan timeSpan;
		[Editable("Uhrzeit")]
		public TimeSpan TimeSpan
		{
			get { return timeSpan; }
			set
			{
				timeSpan = value; date = date.Date.Add(value);
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

		public CalendarEvent(DateTime date, string title)
		{
			this.date = date;
			this.title = title;
			timeSpan = date.TimeOfDay;
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

		public void OnSaveEditing(CalendarEvent oldObj)
		{
			//Delete Item
			LagerClient.CurrentLager.Calendar.RemoveCalendarEvent(oldObj);
			//Insert Calendar Event into correct day
			LagerClient.CurrentLager.Calendar.InsertNewCalendarEvent(this);
		}

		public CalendarEvent CloneDeep()
		{
			return new CalendarEvent(date, title, detail);
		}

		#endregion
	}
}
