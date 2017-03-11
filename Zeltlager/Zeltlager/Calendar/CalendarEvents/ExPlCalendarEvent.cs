using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using Zeltlager.UAM;
namespace Zeltlager.Calendar
{
	[Editable("(expl)Termin")]
	public class ExPlCalendarEvent : CalendarEvent
	{
		PlannedCalendarEvent PlannedEvent { get; set; }

		public ExPlCalendarEvent(PlannedCalendarEvent plannedEvent)
			: base(plannedEvent.Id, DateTime.Now, plannedEvent.Title, plannedEvent.Detail, plannedEvent.GetLager())
		{
			PlannedEvent = plannedEvent;
		}

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, CalendarEvent oldObject)
		{
			PlannedEvent.IsVisible = false;
			DataPacket packet;
			if (oldObject != null)
			{
				throw new Exception("a ExPlannedCalendarEvent can not be edited!!! Serialization will probably break from it..");
				//packet = await EditPacket.Create(serialiser, context, new CalendarEvent(Id, Date, Title, Detail, lager));
			}
			else
				packet = await AddPacket.Create(serialiser, context, new CalendarEvent(Id, Date, Title, Detail, lager));
			await context.LagerClient.AddPacket(packet);
		}
	}
}
