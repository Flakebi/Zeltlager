using System.Threading.Tasks;

namespace Zeltlager
{
	using Client;
	using DataPackets;
	using Serialisation;
	using UAM;
	
	public abstract class Editable<T> : IEditable<T> where T : class
	{
		public virtual async Task OnSaveEditing(LagerClient lager, T oldObject)
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
			Serialiser<LagerClientSerialisationContext> serialiser = lager.ClientSerialiser;
			DataPacket packet;
			if (oldObject != null)
				packet = await EditPacket.Create(serialiser, context, this);
			else
				packet = await AddPacket.Create(serialiser, context, this);
			await context.LagerClient.AddPacket(packet);
		}

		public abstract T Clone();
	}
}
