using System.Threading.Tasks;

namespace Zeltlager
{
	using Client;
	using DataPackets;
	
	public interface IDeletable
	{
		bool IsVisible { get; set; }
	}

	public static class IDeletableHelper
	{
		public static async Task Delete(this IDeletable t, LagerClient lager)
		{
			t.IsVisible = false;
			await lager.AddPacket(await EditPacket.Create(t));
		}
	}
}
