using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using DataPackets;
	using Serialisation;

	/// <summary>
	/// A collaborator of a lager that can add bundles and has an own key.
	/// 
	/// Serialising a collaborator with a LagerSerialisationContext
	/// will write the collaborator data (the signed public key).
	/// Serialising a reference to this collaborator will write the
	/// index of this collaborator in the LagerStatus of the lager.
	/// 
	/// Serialising a collaborator with a LagerClientSerialisationContext
	/// will write the public key of this collaborator (unsigned).
	/// Serialising a reference to this collaborator will write the
	/// PacketId of this collaborator in the collaborator list of the
	/// currently serialising collaborator.
	/// </summary>
	public class Collaborator : ISerialisable<LagerSerialisationContext>, ISerialisable<LagerClientSerialisationContext>
	{
		/// <summary>
		/// The data of this collaborator.
		/// This contains the public key of this collaborator, signed with
		/// the private lager key. This data is signed with the private
		/// key of this collaborator.
		/// </summary>
		byte[] data;

		public KeyPair Key { get; private set; }

		/// <summary>
		/// The list of collaborators (indexed by the id) as of this collaborators view point.
		/// </summary>
		public Dictionary<PacketId, Collaborator> Collaborators { get; private set; }
		List<DataPacketBundle> bundles = new List<DataPacketBundle>();
		public IReadOnlyList<DataPacketBundle> Bundles { get { return bundles; } }

		/// <summary>
		/// Initialises a new collaborator.
		/// </summary>
		/// <param name="id">The id of the collaborator.</param>
		/// <param name="publicKey">
		/// The public key of the collaborator and also the
		/// private key if our own contributor is initialised.
		/// </param>
		public Collaborator(KeyPair key)
		{
			Collaborators = new Dictionary<PacketId, Collaborator>();
			Key = key;
		}

		public Collaborator() : this(new KeyPair())
		{ }

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

		/// <summary>
		/// Verify the key signatures that are stored in the collaborator data.
		/// </summary>
		/// <returns>true, if this collaborator was verified successfully.</returns>
		public Task<bool> Verify(LagerSerialisationContext context)
		{
			//TODO
			return Task.FromResult(true);
		}

		// Serialisation with a LagerSerialisationContext
		public async Task Write(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			if (data == null)
			{
				// Create the data
				MemoryStream mem = new MemoryStream();
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					writer.WritePublicKey(Key);
					byte[] keyData = mem.ToArray();
					// Sign the key with the lager private key
					writer.Write(await LagerBase.CryptoProvider.Sign(context.Lager.AsymmetricKey, keyData));
					keyData = mem.ToArray();
					// Sign the data with our own private key
					writer.Write(await LagerBase.CryptoProvider.Sign(Key, keyData));
				}

				data = mem.ToArray();
			}
			await serialiser.Write(output, context, data);
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			// Get our collaborator id from the LagerStatus
			output.Write((byte)context.Lager.Status.BundleCount.FindIndex(c => c.Item1 == this));
			return new Task(() => { });
		}

		public Task Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			Key = input.ReadPublicKey();
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(mem))
				writer.WritePublicKey(Key);
			byte[] keyData = mem.ToArray();
			data = new byte[keyData.Length + 2 * CryptoConstants.SIGNATURE_LENGTH];
			Array.Copy(keyData, data, keyData.Length);
			// Read the signatures
			Array.Copy(input.ReadBytes(CryptoConstants.SIGNATURE_LENGTH), 0,
				data, keyData.Length, CryptoConstants.SIGNATURE_LENGTH);
			Array.Copy(input.ReadBytes(CryptoConstants.SIGNATURE_LENGTH), 0,
				data, keyData.Length + CryptoConstants.SIGNATURE_LENGTH, CryptoConstants.SIGNATURE_LENGTH);
			return new Task(() => { });
		}

		public static Task<Collaborator> ReadFromId(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			byte id = input.ReadByte();
			return Task.FromResult(context.Lager.Status.BundleCount[id].Item1);
		}

		// Serialisation with a LagerClientSerialisationContext
		public Task Write(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			output.WritePublicKey(Key);
			return new Task(() => { });
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			// Get our collaborator id as seen from the collaborator that writes our id
			return serialiser.WriteId(output, context, context.PacketId.Creator.Collaborators
				.First(c => c.Value == this).Key);
		}

		public Task Read(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			return serialiser.Read(input, context, Key);
		}

		public static async Task<Collaborator> ReadFromId(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			PacketId id = await serialiser.Read(input, context, new PacketId(context.PacketId.Creator));
			return context.PacketId.Creator.Collaborators[id];
		}
	}
}
