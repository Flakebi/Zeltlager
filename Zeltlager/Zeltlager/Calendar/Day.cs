using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
					events = value;
					OnPropertyChanged(nameof(Events));
				}
			}
		}

		private DateTime date;
		public DateTime Date
		{
			get { return date; }
			set
			{
				date = value;
				OnPropertyChanged(nameof(Date));
			}
		}

		public Day(DateTime date)
		{
			Date = date;
			Events = new ObservableCollection<CalendarEvent>();
		}

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
