using System.IO;

namespace Zeltlager
{
	using Calendar;
	using Client;
	using DataPackets;

	public class DeleteCalendarEvent : DataPacket
	{
		public DeleteCalendarEvent() { }

		public DeleteCalendarEvent(CalendarEvent calendarEvent)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(calendarEvent);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				lager.Calendar.RemoveCalendarEvent(input.ReadCalendarEvent());
			}
		}
	}
}
