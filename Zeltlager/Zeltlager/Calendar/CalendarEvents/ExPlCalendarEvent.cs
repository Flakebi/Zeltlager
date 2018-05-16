using System;
using System.Threading.Tasks;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.UAM;
namespace Zeltlager.Calendar
{
	[Editable("Termin    ")]
	public class ExPlCalendarEvent : CalendarEvent
	{
		PlannedCalendarEvent PlannedEvent { get; set; }

		public ExPlCalendarEvent(PlannedCalendarEvent plannedEvent)
			: base(plannedEvent.Id, DateTime.Now, plannedEvent.Title, plannedEvent.Detail, plannedEvent.GetLager())
		{
			PlannedEvent = plannedEvent;
		}

		public override async Task OnSaveEditing(LagerClient lager, PlannedCalendarEvent oldObject)
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
			Serialiser<LagerClientSerialisationContext> serialiser = lager.ClientSerialiser;
			
			if (PlannedEvent.IsVisible)
			{
				PlannedEvent.IsVisible = false;
				DataPacket editPacket = await EditPacket.Create(serialiser, context, PlannedEvent);
				await context.LagerClient.AddPacket(editPacket);
			}
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

		public override PlannedCalendarEvent Clone()
		{
			return new ExPlCalendarEvent(PlannedEvent);
		}
	}
}
