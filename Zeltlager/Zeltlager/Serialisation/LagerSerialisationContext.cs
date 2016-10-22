namespace Zeltlager.Serialisation
{
	using Client;
	using DataPackets;

	public class LagerSerialisationContext
	{
		public LagerBase Lager { get; private set; }
		public PacketId PacketId { get; set; }

		public LagerSerialisationContext(LagerBase lager)
		{
			Lager = lager;
		}
	}

	public class LagerClientSerialisationContext : LagerSerialisationContext
	{
		public LagerClient LagerClient
		{
			get
			{
				return (LagerClient)Lager;
			}
		}

		public LagerClientSerialisationContext(LagerClient lager) : base(lager)
		{ }
	}
}
