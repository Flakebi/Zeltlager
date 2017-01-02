using System;
using System.Threading.Tasks;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace Zeltlager.Calendar
{
	// Editable, not serialisable
	public class ExRefCalendarEvent : CalendarEvent
	{
		public ExRefCalendarEvent()
		{
		}

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, CalendarEvent oldObject)
		{
			DataPacket packet;
			if (oldObject != null)
				packet = await EditPacket.Create(serialiser, context, this);
			else
				packet = await AddPacket.Create(serialiser, context, this);
			await context.LagerClient.AddPacket(packet);
		}
	}
}
