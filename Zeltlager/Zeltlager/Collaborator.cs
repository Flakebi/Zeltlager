using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Cryptography;
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
		/// The id that this collaborator has on the server.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// The data of this collaborator.
		/// This contains the public key of this collaborator, signed with
		/// the private lager key. This data is signed with the private
		/// key of this collaborator.
		/// </summary>
		byte[] data;

		public KeyPair Key { get; private set; }

		Dictionary<PacketId, Collaborator> collaborators = new Dictionary<PacketId, Collaborator>();
		/// <summary>
		/// The list of collaborators (indexed by the id) as of this collaborators view point.
		/// </summary>
		public Dictionary<PacketId, Collaborator> Collaborators => collaborators;
		Dictionary<int, DataPacketBundle> bundles = new Dictionary<int, DataPacketBundle>();
		/// <summary>
		/// The list of bundles of a collaborator indexed by their id.
		/// </summary>
		public IReadOnlyDictionary<int, DataPacketBundle> Bundles => bundles;

		public Collaborator() { }

		/// <summary>
		/// Initialises a new collaborator.
		/// </summary>
		/// <param name="key">
		/// The public key of the collaborator and also the
		/// private key if our own contributor is initialised.
		/// </param>
		public Collaborator(KeyPair key)
		{
			Key = key;
		}

		public void AddBundle(int id, DataPacketBundle bundle)
		{
			if (bundles.ContainsKey(id))
				throw new InvalidOperationException("Can't add a packet bundle with an id that is already taken");
			bundles.Add(id, bundle);
		}

		/// <summary>
		/// Verify the key signatures that are stored in the collaborator data.
		/// Throws an exception if the verification failed.
		/// </summary>
		async Task Verify(LagerSerialisationContext context)
		{
			// Read the signatures
			using (BinaryReader input = new BinaryReader(new MemoryStream(data)))
			{
				input.ReadPublicKey();
				byte[] keyData = new byte[input.BaseStream.Position];
				Array.Copy(data, keyData, keyData.Length);
				byte[] lagerSignature = input.ReadBytes(CryptoConstants.SIGNATURE_LENGTH);
				byte[] signedData = new byte[input.BaseStream.Position];
				Array.Copy(data, signedData, signedData.Length);
				byte[] collaboratorSignature = input.ReadBytes(CryptoConstants.SIGNATURE_LENGTH);

				// Check the signatures
				// Verify the lager signature
				if (!await LagerManager.CryptoProvider.Verify(context.Lager.AsymmetricKey, lagerSignature, keyData))
					throw new InvalidDataException("The lager signature of the collaborator is wrong");
				// Verify the collaborator signature
				if (!await LagerManager.CryptoProvider.Verify(Key, collaboratorSignature, signedData))
					throw new InvalidDataException("The collaborator signature of the collaborator is wrong");
			}
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
					writer.Write(await LagerManager.CryptoProvider.Sign(context.Lager.AsymmetricKey, keyData));
					keyData = mem.ToArray();
					// Sign the data with our own private key
					writer.Write(await LagerManager.CryptoProvider.Sign(Key, keyData));
				}

				data = mem.ToArray();
			}
			await serialiser.Write(output, context, data);
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			// Get our collaborator id from the LagerStatus
			output.Write(Id);
			return Task.WhenAll();
		}

		public Task Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			// Copy the key into the data array
			Key = input.ReadPublicKey();
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(mem))
				writer.WritePublicKey(Key);
			byte[] keyData = mem.ToArray();
			data = new byte[keyData.Length + 2 * CryptoConstants.SIGNATURE_LENGTH];
			Array.Copy(keyData, data, keyData.Length);

			// Copy the signatures
			Array.Copy(input.ReadBytes(CryptoConstants.SIGNATURE_LENGTH * 2), 0,
				data, keyData.Length, CryptoConstants.SIGNATURE_LENGTH * 2);
			return Verify(context);
		}

		public static Task<Collaborator> ReadFromId(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			int id = input.ReadInt32();
			Collaborator collaborator = context.Lager.Collaborators.Values.First(c => c.Id == id);
			// Update the context
			context.PacketId = context.PacketId.Clone(collaborator);
			return Task.FromResult(collaborator);
		}

		// Serialisation with a LagerClientSerialisationContext
		public Task Write(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			output.WritePublicKey(Key);
			return Task.WhenAll();
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			// Get our collaborator id as seen from the collaborator that writes our id
			return serialiser.WriteId(output, context, context.PacketId.Creator.Collaborators
				.First(c => c.Value == this).Key);
		}

		public Task Read(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			Key = input.ReadPublicKey();
			return Task.WhenAll();
		}

		public static async Task<Collaborator> ReadFromId(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			PacketId id = await serialiser.ReadFromId<PacketId>(input, context);
			return context.PacketId.Creator.Collaborators[id];
		}
	}
}
