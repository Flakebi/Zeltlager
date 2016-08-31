using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zeltlager.Calendar
{
	public class Day : INotifyPropertyChanged
	{
		private ObservableCollection<CalendarEvent> events;
		public ObservableCollection<CalendarEvent> Events
		{
			get { return events; }
			set
			{
				if (value != events)
				{
					events = value; OnPropertyChanged("Events");
				}
			}
		}
		private DateTime date;
		public DateTime Date
		{
			get
			{ return date; }
			set
			{
				date = value;
				OnPropertyChanged("Date");
			}
		}


		public Day(DateTime date)
		{
			this.Date = date;
			this.Events = new ObservableCollection<CalendarEvent>();
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
