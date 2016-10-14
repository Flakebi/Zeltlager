using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Zeltlager.DataPackets;

namespace Zeltlager
{
	public class Collaborator
	{
		List<DataPacket> packets = new List<DataPacket>();

		public byte[] Modulus { get; private set; }
		public byte[] PublicKey { get; private set; }
		public byte[] PrivateKey { get; private set; }

		public byte Id { get; private set; }
		public IReadOnlyList<DataPacket> Packets { get { return packets; } }

        /// <summary>
        /// All tent ids should be unique (per collaborator) so the next free
        /// member id for this collaborator is saved here.
        /// </summary>
        public ushort NextMemberId { get; set; }
        public byte NextTentId { get; set; }

        /// <summary>
        /// Initialises a new collaborator.
        /// </summary>
        /// <param name="id">The id of the collaborator.</param>
        /// <param name="publicKey">The public key of the collaborator.</param>
        public Collaborator(byte id, byte[] modulus, byte[] publicKey)
		{
			Id = id;
			Modulus = modulus;
			PublicKey = publicKey;

            NextMemberId = 0;
            NextTentId = 0;
		}

		/// <summary>
		/// Initialises our ownnew collaborator.
		/// </summary>
		/// <param name="id">The id of the collaborator.</param>
		/// <param name="publicKey">The public key of the collaborator.</param>
		/// <param name="privateKey">The private key of the collaborator.</param>
		public Collaborator(byte id, byte[] modulus, byte[] publicKey, byte[] privateKey) : this(id, modulus, publicKey)
		{
			PrivateKey = privateKey;
		}

		public void AddPacket(DataPacket packet) => packets.Add(packet);

		public async Task SaveAll(IIoProvider io, byte[] symmetricKey)
		{
			string id = Id.ToString();
			if (!await io.ExistsFolder(id))
				await io.CreateFolder(id);

			for (ushort i = 0; i < packets.Count; i++)
				await SavePacket(io, symmetricKey, i);
		}

		/// <summary>
		/// Save the packet with the given id and encrypt it using the supplied symmetric key.
		/// </summary>
		/// <param name="io"></param>
		/// <param name="symmetricKey">The key to encrypt the packet.</param>
		/// <param name="i">The packet id.</param>
		/// <returns></returns>
		public async Task SavePacket(IIoProvider io, byte[] symmetricKey, ushort i)
		{
			var packet = packets[i];

			byte[] data;
			// Get packet data
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(mem))
			{
				// Write the packet id
				writer.Write(i.ToBytes());
				packet.WritePacket(writer);
			}

			data = mem.ToArray();

			if (packet.Iv == null)
				// Generate iv
				packet.Iv = await LagerBase.CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);

			// Write iv and encrypted data
			mem = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(mem))
			{
				// Write iv
				writer.Write(packet.Iv);
				// Write encrypted packet data
				writer.Write(await LagerBase.CryptoProvider.EncryptSymetric(symmetricKey, packet.Iv, data));
			}
			data = mem.ToArray();

			if (packet.Signature == null)
			{
				if (packet is InvalidDataPacket)
				{
					// Just write the packet data
					using (BinaryWriter output = new BinaryWriter(await io.WriteFile(Path.Combine(Id.ToString(), i.ToString()))))
						packet.WritePacket(output);
					return;
				} else if (PrivateKey != null)
					// Generate signature
					packet.Signature = await LagerBase.CryptoProvider.Sign(Modulus, PrivateKey, data);
				else
					throw new InvalidOperationException("Found unencrypted packet without private key.");
			}

			if (packet.Signature != null)
			{
				using (BinaryWriter output = new BinaryWriter(await io.WriteFile(Path.Combine(Id.ToString(), i.ToString()))))
				{
					// Write signature
					output.Write(packet.Signature);
					// Write iv and encrypted data
					output.Write(data);
				}
			}
		}

		/// <summary>
		/// Loads all packages of this collaborator.
		/// </summary>
		/// <param name="io">The io provider.</param>
		/// <param name="symmetricKey">The symmetric key for the decryption of the pakcets.</param>
		/// <param name="lager">The lager where this collaborator belongs to.</param>
		/// <param name="version">The version of the saved packets.</param>
		/// <param name="packetCount">The amount of packets that this contributor has.</param>
		/// <returns>True if everything was loaded successfully, false otherwise.</returns>
		public async Task<bool> Load(IIoProvider io, byte[] symmetricKey, LagerBase lager, byte version, ushort packetCount)
		{
			bool success = true;
			string id = Id.ToString();
			for (ushort i = 0; i < packetCount; i++)
			{
				byte[] signature;
				byte[] allData;
				try
				{
					using (BinaryReader input = new BinaryReader(await io.ReadFile(Path.Combine(id, i.ToString()))))
					{
						// Read signature
						signature = input.ReadBytes(CryptoConstants.SIGNATURE_LENGTH);
						var length = (int)(input.BaseStream.Length - input.BaseStream.Position);
						allData = input.ReadBytes(length);
					}

					// Verify signature
					if (!await LagerBase.CryptoProvider.Verify(Modulus, signature, allData))
						// The packet has an invalid signature
						throw new Exception("The packet has an invalid signature.");

					byte[] iv = new byte[CryptoConstants.IV_LENGTH];
					Array.Copy(allData, 0, iv, 0, iv.Length);
					byte[] data = new byte[allData.Length - iv.Length];
					Array.Copy(allData, iv.Length, data, 0, data.Length);

					data = await LagerBase.CryptoProvider.DecryptSymetric(symmetricKey, iv, data);
					// Check if the right id
					ushort packetId = data.ToUShort(0);
					if (packetId != i)
						throw new Exception("The packet has an invalid id.");

					DataPacket packet = DataPacket.ReadPacket(data.Skip(2).ToArray());
					packet.Iv = iv;
					packet.Signature = signature;
					packets.Add(packet);
				} catch (Exception e)
				{
					success = false;
					// Log the exception
					await LagerBase.Log.Exception("Collaborator", e);
					lager.MissingPackets.Add(new Tuple<byte, ushort>(Id, i));
					// Try to open the file
					try
					{
						using (BinaryReader input = new BinaryReader(await io.ReadFile(Path.Combine(id, i.ToString()))))
						{
							// Read all data
							allData = input.ReadBytes((int)input.BaseStream.Length);
							packets.Add(new InvalidDataPacket(allData));
						}
					} catch (Exception)
					{
						// Insert an empty invalid packet
						packets.Add(new InvalidDataPacket(new byte[0]));
					}
				}
			}
			return success;
		}
	}
}
