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
		ReferenceCalendarEvent Reference { get; set; }

		public ExRefCalendarEvent(ReferenceCalendarEvent reference)
			: base(reference.Id, reference.Date, reference.Reference.Title, reference.Reference.Detail, reference.Reference.GetLager())
		{
			Reference = reference;
		}
		
		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, CalendarEvent oldObject)
		{
			Reference.makeInvisible();
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
