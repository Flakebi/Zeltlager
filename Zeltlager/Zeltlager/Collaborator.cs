using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

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
		/// The data of this collaborator.
		/// This contains the public key of this collaborator, signed with
		/// the private lager key. This data is signed with the private
		/// key of this collaborator.
		/// </summary>
		byte[] data;

		public KeyPair Key { get; set; }

		List<DataPacketBundle> bundles = new List<DataPacketBundle>();
		/// <summary>
		/// The list of bundles of a collaborator indexed by their id.
		/// </summary>
		public IReadOnlyList<DataPacketBundle> Bundles => bundles;

		public Collaborator() { }

		/// <summary>
		/// Initialises a new collaborator.
		/// </summary>
		/// <param name="key">
		/// The public key of the collaborator and also the
		/// private key if our own collaborator is initialised.
		/// </param>
		public Collaborator(KeyPair key)
		{
			Key = key;
		}

		public void AddBundle(DataPacketBundle bundle)
		{
			bundle.Id = bundles.Count;
			bundles.Add(bundle);
		}

		/// <summary>
		/// Unload all bundles from this collaborator.
		/// </summary>
		public void Unload()
		{
			bundles.Clear();
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
				if (!await LagerManager.CryptoProvider.Verify(context.Lager.Data.AsymmetricKey, lagerSignature, keyData))
					throw new LagerException("The lager signature of the collaborator is wrong");
				// Verify the collaborator signature
				if (!await LagerManager.CryptoProvider.Verify(Key, collaboratorSignature, signedData))
					throw new LagerException("The collaborator signature of the collaborator is wrong");
			}
		}

		public override string ToString()
		{
			string mod = Key.Modulus.ToHexString();
			return mod.Substring(mod.Length - 5);
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
					writer.Write(await LagerManager.CryptoProvider.Sign(context.Lager.Data.AsymmetricKey, keyData));
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
			throw new InvalidOperationException("You can't write the id of a collaborator");
		}

		public async Task Read(BinaryReader input,
			Serialiser<LagerSerialisationContext> serialiser,
			LagerSerialisationContext context)
		{
			data = await serialiser.Read<byte[]>(input, context, null);
			// Read the key
			MemoryStream mem = new MemoryStream(data);
			using (BinaryReader reader = new BinaryReader(mem))
				Key = reader.ReadPublicKey();
			await Verify(context);
		}

		public static Task<Collaborator> ReadFromId(BinaryReader input,
			Serialiser<LagerSerialisationContext> serialiser,
			LagerSerialisationContext context)
		{
			throw new InvalidOperationException("You can't read the id of a collaborator");
		}

		// Serialisation with a LagerClientSerialisationContext
		public Task Write(BinaryWriter output,
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			output.WritePublicKey(Key);
			return Task.WhenAll();
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			output.WritePublicKey(Key);
			return Task.WhenAll();
		}

		public Task Read(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			Key = input.ReadPublicKey();
			return Task.WhenAll();
		}

		public static Task<Collaborator> ReadFromId(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			KeyPair key = input.ReadPublicKey();
			return Task.FromResult(context.Lager.Collaborators[key]);
		}
	}
}
