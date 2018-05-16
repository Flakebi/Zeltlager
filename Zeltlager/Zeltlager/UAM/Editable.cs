using System.Threading.Tasks;

namespace Zeltlager
{
	using Client;
	using DataPackets;
		using UAM;
	
	public abstract class Editable<T> : IEditable<T> where T : class
	{
		public virtual async Task OnSaveEditing(LagerClient lager, T oldObject)
		{
			DataPacket packet;
			if (oldObject != null)
				packet = await EditPacket.Create(this);
			else
				packet = await AddPacket.Create(this);
			await lager.AddPacket(packet);
		}

		public abstract T Clone();
	}
}
