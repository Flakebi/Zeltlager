using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zeltlager.Calendar
{
	public class CalendarEvent : INotifyPropertyChanged
	{
		private DateTime date;
		public DateTime Date
		{
			get { return date; }
			set { date = value; OnPropertyChanged("Date"); }
		}
		private TimeSpan timeSpan;
		public TimeSpan TimeSpan
		{
			get { return timeSpan; }
			set { timeSpan = value; date = date.Date.Add(value); 
				OnPropertyChanged("TimeSpan"); OnPropertyChanged("TimeString"); OnPropertyChanged("Date"); }
		}
		public string TimeString
		{
			get { return date.ToString("HH:mm"); }
		}
		private string title;
		public string Title
		{
			get { return title; }
			set { title = value; OnPropertyChanged("Title"); }
		}
		private string detail;
		public string Detail
		{
			get { return detail; }
			set { detail = value; OnPropertyChanged("Detail"); }
		}

		public CalendarEvent(DateTime date, string title, string detail)
		{
			this.date = date;
			this.title = title;
			this.detail = detail;
			timeSpan = date.TimeOfDay;
		}
		public CalendarEvent(DateTime date, string title)
		{
			this.date = date;
			this.title = title;
			timeSpan = date.TimeOfDay;
		}

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName]string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
