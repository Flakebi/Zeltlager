using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Serialisation;

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
	public class PacketId : IEquatable<PacketId>, ISerialisable<LagerClientSerialisationContext>
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

		// Serialisation
		public Task Write(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			//TODO
			return new Task(() => { });
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			// Get our collaborator id as seen from the collaborator that writes our id
			//TODO
			return new Task(() => { });
		}

		public Task Read(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			//TODO
			return new Task(() => { });
		}

		public static PacketId ReadFromId(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			//TODO
			return null;
		}
	}
}
