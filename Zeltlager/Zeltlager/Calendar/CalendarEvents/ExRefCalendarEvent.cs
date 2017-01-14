using System;
using System.Threading.Tasks;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using Zeltlager.UAM;

namespace Zeltlager.Calendar
{
	// Editable, not serialisable
	[Editable("(exref)Termin")]
	public class ExRefCalendarEvent : CalendarEvent
	{
		ReferenceCalendarEvent reference;

		public ExRefCalendarEvent(PacketId id, DateTime date, string title, string detail, ReferenceCalendarEvent reference, Client.LagerClient lager)
			: base(id, date, title, detail, lager)
		{
			this.reference = reference;
		}
		
		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, CalendarEvent oldObject)
		{
			reference.makeInvisible();
			DataPacket packet;
			if (oldObject != null)
			{
				throw new Exception("a ReferenceCalendarEvent can not be added!!! Serialization will probably break from it..");
				//packet = await EditPacket.Create(serialiser, context, new CalendarEvent(Id, Date, Title, Detail, lager));
			}
			else
				packet = await AddPacket.Create(serialiser, context, new CalendarEvent(Id, Date, Title, Detail, lager));
			await context.LagerClient.AddPacket(packet);
		}
	}
}
