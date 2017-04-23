using System;
using System.Threading.Tasks;
using Zeltlager.Client;
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
		
		public override async Task OnSaveEditing(LagerClient lager, PlannedCalendarEvent oldObject)
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
			Serialiser<LagerClientSerialisationContext> serialiser = lager.ClientSerialiser;
			Reference.IsVisible = false;
			DataPacket packet;
			if (oldObject == null)
			{
				throw new Exception("a ReferenceCalendarEvent can not be added!!! Serialization will probably break from it..");
				//packet = await EditPacket.Create(serialiser, context, new CalendarEvent(Id, Date, Title, Detail, lager));
			}
			else
				packet = await AddPacket.Create(serialiser, context, new CalendarEvent(Id, Date, Title, Detail, lager));
			await context.LagerClient.AddPacket(packet);
		}

		public override PlannedCalendarEvent Clone()
		{
			return new ExRefCalendarEvent(this.Reference);
		}
	}
}
