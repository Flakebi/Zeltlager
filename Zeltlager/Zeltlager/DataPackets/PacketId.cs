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
		public Collaborator Creator { get; private set; }
		[Serialisation(Type = SerialisationType.Reference)]
		public DataPacketBundle Bundle { get; private set; }
		[Serialisation]
		public int? PacketIndex { get; private set; }

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
			return PacketIndex.GetHashCode() ^ Creator.Key.Modulus[0].GetHashCode() ^ Bundle.Id.GetHashCode();
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
		public async Task Write(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			await serialiser.WriteId(output, context, Creator);
			await serialiser.WriteId(output, context, this);
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			output.Write(Bundle.Id);
			output.Write(PacketIndex.Value);
			return new Task(() => { });
		}

		public async Task Read(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			var id = await serialiser.ReadFromId<PacketId>(input, context);
			Bundle = id.Bundle;
			PacketIndex = id.PacketIndex;
			Creator = await serialiser.ReadFromId<Collaborator>(input, context);
		}

		public static Task<PacketId> ReadFromId(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			int packetIndex = input.ReadInt32();
			int bundleId = input.ReadInt32();
			var collaborator = context.PacketId.Creator;
			PacketId id = new PacketId(collaborator, collaborator.Bundles[bundleId], packetIndex);
			return Task.FromResult(id);
		}
	}
}
