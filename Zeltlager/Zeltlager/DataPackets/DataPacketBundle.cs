using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	/// <summary>
	/// Represents a bundle of encrypted packets.
	/// </summary>
	public class DataPacketBundle
	{
		/// <summary>
		/// The maximum number of packets in one bundle.
		/// </summary>
		public const byte MAX_PACKETS = 255;

		public uint Id { get; set; }
		List<DataPacket> packets;
		public IReadOnlyList<DataPacket> Packets { get { return packets; } }
		/// <summary>
		/// The full encrypted data of the bundle.
		/// </summary>
		byte[] data;

		public DataPacketBundle() { }

		public DataPacketBundle(DataPacket[] packets)
		{
			this.packets = new List<DataPacket>(packets);
		}

		public DataPacketBundle(byte[] data)
		{
			this.data = data;
		}

		/// <summary>
		/// Packs and compresses all packets into an unencrypted byte array.
		/// </summary>
		/// <returns>The packed packets.</returns>
		byte[] Pack()
		{
			MemoryStream mem = new MemoryStream();
			// Compress the data using gzip
			using (BinaryWriter output = new BinaryWriter(new GZipStream(mem, CompressionLevel.Optimal)))
			{
				output.Write((byte)packets.Count);
				foreach (var packet in packets)
				{
					MemoryStream tmp = new MemoryStream();
					using (BinaryWriter tmpOut = new BinaryWriter(tmp))
						packet.WritePacket(tmpOut);
					byte[] data = tmp.ToArray();
					output.Write(data.Length);
					output.Write(data);
				}
			}
			return mem.ToArray();
		}

		/// <summary>
		/// Unpacks and decompresses packets from an unencrypted byte array.
		/// </summary>
		/// <param name="data">The byte array that contains the packets.</param>
		void Unpack(byte[] data)
		{
			MemoryStream mem = new MemoryStream(data);
			using (BinaryReader input = new BinaryReader(new GZipStream(mem, CompressionMode.Decompress)))
			{
				byte count = input.ReadByte();
				packets = new List<DataPacket>(count);
				for (ushort i = 0; i < count; i++)
				{
					int length = input.ReadInt32();
					byte[] bs = input.ReadBytes(length);
					packets.Add(DataPacket.ReadPacket(bs));
				}
			}
		}

		/// <summary>
		/// Encrypts and writes all packets into the data array.
		/// </summary>
		async Task Serialise(LagerClientSerialisationContext context)
		{
			// Get the unencrypted data
			byte[] packets = Pack();

			byte[] iv = await LagerBase.CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);

			// Write id, iv and encrypted data
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				// Write the bundle id
				output.Write(Id);
				// Write iv
				output.Write(iv);
				// Write encrypted packet data
				output.Write(await LagerBase.CryptoProvider.EncryptSymetric(context.LagerClient.SymmetricKey, iv, data));
			}
			byte[] encryptedData = mem.ToArray();

			// Generate signature
			byte[] signature = await LagerBase.CryptoProvider.Sign(context.PacketId.Creator.Key, encryptedData);

			// Write all data
			mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(signature);
				output.Write(encryptedData);
			}
			data = mem.ToArray();
		}

		public async Task Write(LagerClientSerialisationContext context, BinaryWriter output)
		{
			if (data == null)
				await Serialise(context);
			output.Write(data);
		}

		/// <summary>
		/// Verifies the packet signature and id.
		/// </summary>
		/// <param name="context">The context for the verification.</param>
		/// <returns>The packet iv und encrypted data.</returns>
		public async Task<Tuple<byte[], byte[]>> VerifyAndGetEncryptedData(LagerSerialisationContext context)
		{
			byte[] signature = new byte[CryptoConstants.SIGNATURE_LENGTH];
			byte[] encryptedData = new byte[data.Length - CryptoConstants.SIGNATURE_LENGTH];

			// Read signature and the encrypted data
			Array.Copy(data, signature, signature.Length);
			Array.Copy(data, signature.Length, encryptedData, 0, encryptedData.Length);

			// Verify signature
			if (!await LagerBase.CryptoProvider.Verify(context.PacketId.Creator.Key, signature, encryptedData))
				throw new Exception("The bundle has an invalid signature.");

			MemoryStream mem = new MemoryStream(encryptedData);
			using (BinaryReader input = new BinaryReader(mem))
			{
				// Check if the bundle has the right id
				if (input.ReadUInt32() != Id)
					throw new Exception("The bundle has an invalid id.");
				byte[] iv = input.ReadBytes(CryptoConstants.IV_LENGTH);
				return new Tuple<byte[], byte[]>(iv,
					input.ReadBytes((int)(input.BaseStream.Length - input.BaseStream.Position)));
			}
		}

		async Task Deserialise(LagerClientSerialisationContext context)
		{
			byte[] signature = new byte[CryptoConstants.SIGNATURE_LENGTH];
			var verificationResult = await VerifyAndGetEncryptedData(context);
			byte[] unencryptedData = await LagerBase.CryptoProvider.DecryptSymetric(
				context.LagerClient.SymmetricKey, verificationResult.Item1, verificationResult.Item2);
			Unpack(unencryptedData);
		}

		public async Task<Tuple<PacketId, DataPacket>[]> GetPackets(LagerClientSerialisationContext context)
		{
			if (packets == null)
				await Deserialise(context);

			Tuple<PacketId, DataPacket>[] result = new Tuple<PacketId, DataPacket>[packets.Count];
			PacketId id = context.PacketId.Clone(this);
			for (int i = 0; i < packets.Count; i++)
			{
				id = id.Clone((byte)i);
				result[i] = new Tuple<PacketId, DataPacket>(id, packets[i]);
			}
			return result;
		}

		public void AddPacket(DataPacket packet)
		{
			data = null;
			if (packets == null)
				packets = new List<DataPackets.DataPacket>();
			if (packets.Count == MAX_PACKETS)
				throw new InvalidOperationException("The bundle can't contain more packets");
			packets.Add(packet);
		}
	}
}
