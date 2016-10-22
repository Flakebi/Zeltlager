using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using DataPackets;
	using Serialisation;

	public class Collaborator : ISerialisable<LagerSerialisationContext>
	{
		List<DataPacketBundle> bundles = new List<DataPacketBundle>();

		public KeyPair Key { get; private set; }

		public byte Id { get; private set; }
		/// <summary>
		/// The list of collaborators (indexed by the id) as of this collaborators view point.
		/// </summary>
		public Dictionary<Collaborator, PacketId> Collaborators { get; private set; }
		public IReadOnlyList<DataPacketBundle> Bundles { get { return bundles; } }

		/// <summary>
		/// Initialises a new collaborator.
		/// </summary>
		/// <param name="id">The id of the collaborator.</param>
		/// <param name="publicKey">
		/// The public key of the collaborator and also the
		/// private key if our own contributor is initialised.
		/// </param>
		public Collaborator(byte id, KeyPair key)
		{
			Collaborators = new Dictionary<Collaborator, PacketId>();
			Collaborators[this] = new PacketId(this); //TODO Is that all?
			Id = id;
			Key = key;
		}

		public void AddBundle(DataPacketBundle bundle) => bundles.Add(bundle);

		public async Task SaveAll(IIoProvider io, byte[] symmetricKey)
		{
			string id = Id.ToString();
			if (!await io.ExistsFolder(id))
				await io.CreateFolder(id);

			for (ushort i = 0; i < bundles.Count; i++)
				;
			//TODO
			//await SavePacket(io, symmetricKey, i);
		}

		// Serialisation
		public void Write(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			serialiser.Write(output, context, Key);
			//TODO Write signatures
		}

		public void WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			// Get our collaborator id as seen from the collaborator that writes our id
			serialiser.Write(output, context, context.PacketId.Creator.Collaborators[this].PacketIndex.Value);
		}

		public void Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			serialiser.Read(input, context, Key);
			//TODO Read signatures
		}

		public static Collaborator ReadFromId(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			byte id = serialiser.Read<byte>(input, context, 0);
			return context.Collaborator.Collaborators[id];
		}
	}
}
