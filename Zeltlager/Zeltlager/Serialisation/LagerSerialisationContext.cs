using System.Collections.Generic;

namespace Zeltlager.Serialisation
{
	using Client;
	using DataPackets;

	public class LagerSerialisationContext
	{
		public LagerManager Manager { get; private set; }
		public LagerBase Lager { get; private set; }
		public PacketId PacketId { get; set; }

		public LagerSerialisationContext(LagerBase lager)
		{
			Manager = lager.Manager;
			Lager = lager;
		}
	}

	public class LagerClientSerialisationContext : LagerSerialisationContext
	{
		/// <summary>
		/// The list of packets that will be applied.
		/// The packet at the end of this list will be applied next and the removed.
		/// </summary>
		public List<DataPacket> Packets;

		public LagerClient LagerClient
		{
			get
			{
				return (LagerClient)Lager;
			}
		}

		public LagerClientSerialisationContext(LagerClient lager) : base(lager) { }
	}
}
