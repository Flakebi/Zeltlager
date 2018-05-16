using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	/// <summary>
	/// A PacketId idenfifies a packet written by a collaborator and
	/// stored in a packet bundle.
	/// 
	/// Serialising a PacketId with a LagerClientSerialisationContext
	/// will write the collaborator id of the creator of the packet
	/// and the bundle and packet id.
	/// Serialising a reference to this PacketId will write only the
	/// bundle and packet id.
	/// </summary>
	public class PacketId : IEquatable<PacketId>
	{
		// todo json ref
		public Collaborator Creator { get; private set; }
		// todo json ref
		public DataPacketBundle Bundle { get; private set; }
		public int? PacketIndex { get; private set; }
		public DataPacket Packet => Bundle.Packets[PacketIndex.Value];

		protected PacketId() { }

		public PacketId(Collaborator creator, DataPacketBundle bundle = null, int? packetIndex = null)
		{
			Creator = creator;
			Bundle = bundle;
			PacketIndex = packetIndex;
		}

		public PacketId Clone()
		{
			return new PacketId(Creator, Bundle, PacketIndex);
		}

		public PacketId Clone(Collaborator creator)
		{
			return new PacketId(creator, null, null);
		}

		public PacketId Clone(DataPacketBundle bundle)
		{
			return new PacketId(Creator, bundle, null);
		}

		public PacketId Clone(int packetIndex)
		{
			return new PacketId(Creator, Bundle, packetIndex);
		}

		public override string ToString()
		{
			return string.Format("Creator {0}, Bundle {1}, Packet {2}", Creator, Bundle.Id, PacketIndex);
		}

		public bool Equals(PacketId other)
		{
			if (other == null)
				return false;
			return Creator == other.Creator && Bundle == other.Bundle && PacketIndex == other.PacketIndex;
		}

		public override bool Equals(object obj)
		{
			PacketId other = obj as PacketId;
			if (((object)other) == null)
				return false;
			return Equals(other);
		}

		public override int GetHashCode()
		{
			return PacketIndex.GetHashCode() ^
				(Creator == null ? 0 : Creator.Key.Modulus[0].GetHashCode()) ^
				(Bundle == null ? 0 : Bundle.Id.GetHashCode());
		}

		public static bool operator ==(PacketId p1, PacketId p2)
		{
			if ((object)p1 == null)
				return (object)p2 == null;
			// Don't get into an endless loop with Equals
			if ((object)p2 == null)
				return false;
			return p1.Equals(p2);
		}

		public static bool operator !=(PacketId p1, PacketId p2)
		{
			return !(p1 == p2);
		}
	}
}
