using System;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Calendar;
using System.IO;

namespace Zeltlager
{
	public class AddCalendarEvent : DataPacket
	{
		public AddCalendarEvent() {}

		public AddCalendarEvent(CalendarEvent calendarEvent)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(calendarEvent);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				lager.Calendar.InsertNewCalendarEvent(input.ReadCalendarEvent());
			}
		}
	}
}
