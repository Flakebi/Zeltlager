using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zeltlager.Calendar
{
	public class CalendarEvent : INotifyPropertyChanged
	{
		private DateTime time;
		public DateTime Time
		{
			get { return time; }
			set { time = value; OnPropertyChanged("Time"); }
		}
		private TimeSpan timeSpan;
		public TimeSpan TimeSpan
		{
			get { return timeSpan;}
			set { timeSpan = value; time = time.Date.Add(value); OnPropertyChanged("TimeSpan"); OnPropertyChanged("TimeString"); }
		}
		public string TimeString
		{
			get { return time.ToString("HH:mm"); }
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

		public CalendarEvent(DateTime time, string title, string detail)
		{
			this.time = time;
			this.title = title;
			this.detail = detail;
			timeSpan = time.TimeOfDay;
		}
		public CalendarEvent(DateTime time, string title)
		{
			this.time = time;
			this.title = title;
			timeSpan = time.TimeOfDay;
		}

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName]string propertyName = null)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion
	}
}

