using System;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	public class PacketId : IEquatable<PacketId>
	{
		[Serialisation(Type = SerialisationType.Reference)]
		public readonly Collaborator Creator;
		[Serialisation(Type = SerialisationType.Reference)]
		public readonly DataPacketBundle Bundle;
		[Serialisation]
		public readonly byte? PacketIndex;

		public PacketId(Collaborator creator, DataPacketBundle bundle = null, byte? packetIndex = null)
		{
			Creator = creator;
			Bundle = bundle;
			PacketIndex = packetIndex;
		}

		public PacketId Clone(Collaborator creator)
		{
			return new PacketId(creator, null, null);
		}

		public PacketId Clone(DataPacketBundle bundle)
		{
			return new PacketId(Creator, bundle, null);
		}

		public PacketId Clone(byte packetIndex)
		{
			return new PacketId(Creator, Bundle, packetIndex);
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
			if (other == null)
				return false;
			return Equals(other);
		}

		public override int GetHashCode()
		{
			return PacketIndex.GetHashCode() ^ Creator.Key.PublicKey.GetHashCode() ^ Bundle.Id.GetHashCode();
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
