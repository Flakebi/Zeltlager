namespace Zeltlager.Serialisation
{
	using Client;
	using DataPackets;

	public class LagerSerialisationContext
	{
		public LagerManager Manager { get; private set; }
		public LagerBase Lager { get; private set; }
		public PacketId PacketId { get; set; }

		public LagerSerialisationContext(LagerManager manager, LagerBase lager)
		{
			Manager = manager;
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

		public LagerClientSerialisationContext(LagerManager manager, LagerClient lager) : base(manager, lager)
		{ }
	}
}
