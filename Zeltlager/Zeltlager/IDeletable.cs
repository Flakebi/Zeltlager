using System.Threading.Tasks;

namespace Zeltlager
{
	using Client;
	using DataPackets;
	using Serialisation;

	public interface IDeletable
	{
		[Serialisation]
		bool IsVisible { get; set; }
	}

	public static class IDeletableHelper
	{
		public static async Task Delete(this IDeletable t, LagerClient lager)
		{
			t.IsVisible = false;
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
			Serialiser<LagerClientSerialisationContext> serialiser = lager.ClientSerialiser;
			await context.LagerClient.AddPacket(await EditPacket.Create(serialiser, context, t));
		}
	}
}
