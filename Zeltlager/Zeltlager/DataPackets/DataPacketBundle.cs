using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Cryptography;

	/// <summary>
	/// Represents a bundle of encrypted packets.
	///
	/// Serialising a DataPacketBundle will write its data.
	/// Only serialise a DataPacketBundle with a LagerSerialisationContext.
	/// </summary>
	public class DataPacketBundle
	{
		/// <summary>
		/// The maximum number of bytes in one bundle
		/// (except if it contains only one packet).
		/// </summary>
		public const int MAX_PACKET_SIZE = 1024;

		public int Id { get; set; }

		List<DataPacket> packets;
		/// <summary>
		/// The list of packets contained in this bundle.
		/// The index of a packet is also its id. If a packet could not
		/// be deserialised, it is added as an invalid packet.
		/// </summary>
		public IReadOnlyList<DataPacket> Packets { get { return packets; } }

		/// <summary>
		/// The full encrypted data of the bundle.
		/// </summary>
		byte[] data;

		public int Size => data.Length;

		public DataPacketBundle()
		{
			data = new byte[0];
		}

		public DataPacketBundle(byte[] data)
		{
			this.data = data;
		}

		/// <summary>
		/// Packs and compresses all packets into an unencrypted byte array.
		/// </summary>
		/// <returns>The packed packets.</returns>
		public byte[] Pack()
		{
			MemoryStream mem = new MemoryStream();
			// Compress the data using gzip
			using (BinaryWriter output = new BinaryWriter(new GZipStream(mem, CompressionLevel.Optimal)))
			{
				output.Write(packets.Count);
				foreach (var packet in packets)
				{
					MemoryStream tmp = new MemoryStream();
					using (BinaryWriter tmpOut = new BinaryWriter(tmp))
						packet.WritePacket(tmpOut);
					byte[] tmpData = tmp.ToArray();
					output.Write(tmpData.Length);
					output.Write(tmpData);
				}
			}
			return mem.ToArray();
		}

		/// <summary>
		/// Unpacks and decompresses packets from an unencrypted byte array.
		/// </summary>
		/// <param name="unencryptedData">The byte array that contains the packets.</param>
		void Unpack(byte[] unencryptedData)
		{
			PacketId id = context.PacketId.Clone(this);
			context.PacketId = id.Clone();
			MemoryStream mem = new MemoryStream(unencryptedData);
			using (BinaryReader input = new BinaryReader(new GZipStream(mem, CompressionMode.Decompress)))
			{
				int count = input.ReadInt32();
				packets = new List<DataPacket>(count);
				for (int i = 0; i < count; i++)
				{
					int length = input.ReadInt32();
					byte[] bs = input.ReadBytes(length);
					id = id.Clone(i);
					packets.Add(DataPacket.ReadPacket(id, bs));
				}
			}
		}

		/// <summary>
		/// Encrypts and writes all packets into the data array.
		/// </summary>
		public async Task Serialise()
		{
			// Get the unencrypted data
			byte[] packed = Pack();

			byte[] iv = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);

			// Write id, iv and encrypted data
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				// Write the bundle id
				output.Write(Id);
				// Write iv
				output.Write(iv);
				// Write encrypted packet data
				output.Write(await LagerManager.CryptoProvider.EncryptSymetric(context.LagerClient.Data.SymmetricKey, iv, packed));
			}
			byte[] encryptedData = mem.ToArray();

			// Generate signature
			byte[] signature = await LagerManager.CryptoProvider.Sign(context.PacketId.Creator.Key, encryptedData);

			// Write all data
			mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(signature);
				output.Write(encryptedData);
			}
			data = mem.ToArray();
		}

		/// <summary>
		/// Verify the signature of this bundle.
		/// This function throws an exception if it can't be verified successfully.
		/// </summary>
		/// <param name="creator">The creator of this bundle.</param>
		public async Task Verify(Collaborator creator)
		{
			byte[] signature = new byte[CryptoConstants.SIGNATURE_LENGTH];
			byte[] encryptedData = new byte[data.Length - CryptoConstants.SIGNATURE_LENGTH];

			// Read signature and the encrypted data
			Array.Copy(data, signature, signature.Length);
			Array.Copy(data, signature.Length, encryptedData, 0, encryptedData.Length);

			// Verify signature
			if (!await LagerManager.CryptoProvider.Verify(creator.Key, signature, encryptedData))
				throw new LagerException("The bundle has an invalid signature.");

			// Check the id
			using (BinaryReader input = new BinaryReader(new MemoryStream(encryptedData)))
			{
				// Check if the bundle has the right id
				int readId = input.ReadInt32();
				if (readId != Id)
					throw new LagerException("The bundle has an invalid id.");
			}
		}

		/// <summary>
		/// Verifies the packet signature and id.
		/// </summary>
		/// <param name="context">The context for the verification.</param>
		/// <returns>The packet iv und encrypted data.</returns>
		public async Task<Tuple<byte[], byte[]>> VerifyAndGetEncryptedData()
		{
			await Verify(context.PacketId.Creator);
			
			byte[] encryptedData = new byte[data.Length - CryptoConstants.SIGNATURE_LENGTH];

			// Read the encrypted data
			Array.Copy(data, CryptoConstants.SIGNATURE_LENGTH, encryptedData, 0, encryptedData.Length);

			MemoryStream mem = new MemoryStream(encryptedData);
			using (BinaryReader input = new BinaryReader(mem))
			{
				// Check if the bundle has the right id
				int readId = input.ReadInt32();
				if (readId != Id)
					throw new LagerException("The bundle has an invalid id.");
				byte[] iv = input.ReadBytes(CryptoConstants.IV_LENGTH);
				return new Tuple<byte[], byte[]>(iv,
					input.ReadBytes((int)(input.BaseStream.Length - input.BaseStream.Position)));
			}
		}

		async Task Deserialise()
		{
			var verificationResult = await VerifyAndGetEncryptedData(context);
			byte[] unencryptedData = await LagerManager.CryptoProvider.DecryptSymetric(
				context.LagerClient.Data.SymmetricKey, verificationResult.Item1, verificationResult.Item2);
			Unpack(context, unencryptedData);
		}

		public async Task<IReadOnlyList<DataPacket>> GetPackets()
		{
			if (packets == null)
				await Deserialise(context);
			return packets;
		}

		public async Task AddPacket(DataPacket packet)
		{
			if (packets == null)
				packets = new List<DataPacket>();
			if (packets.Count != 0 && Size >= MAX_PACKET_SIZE)
				throw new InvalidOperationException("The bundle can't contain more packets");
			// Make sure that the id of the packets are set
			PacketId id = context.PacketId.Clone(this);
			packet.Id = id.Clone(packets.Count);
			packets.Add(packet);
			await Serialise(context);
		}
	}
}
