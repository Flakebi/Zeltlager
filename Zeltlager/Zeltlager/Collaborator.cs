using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zeltlager.DataPackets;

namespace Zeltlager
{
	public class Collaborator
	{
		List<DataPacket> packets = new List<DataPacket>();

		byte[] publicKey;
		byte[] privateKey;

		public byte Id { get; private set; }
		public IReadOnlyList<DataPacket> Packets { get { return packets; } }

		/// <summary>
		/// Initialises a new collaborator.
		/// </summary>
		/// <param name="id">The id of the collaborator.</param>
		/// <param name="publicKey">The public key of the collaborator.</param>
		public Collaborator(byte id, byte[] publicKey)
		{
			Id = id;
			this.publicKey = publicKey;
		}

		/// <summary>
		/// Initialises our ownnew collaborator.
		/// </summary>
		/// <param name="id">The id of the collaborator.</param>
		/// <param name="publicKey">The public key of the collaborator.</param>
		/// <param name="privateKey">The private key of the collaborator.</param>
		public Collaborator(byte id, byte[] publicKey, byte[] privateKey) : this(id, publicKey)
		{
			this.privateKey = privateKey;
		}

		public void AddPacket(DataPacket packet)
		{
			packets.Add(packet);
		}

		public async Task Save(IIoProvider io, byte[] symmetricKey)
		{
			string id = Id.ToString();
			if (!await io.ExistsFolder(id))
				await io.CreateFolder(id);

			for (int i = 0; i < packets.Count; i++)
			{
				var output = await io.WriteFile(Path.Combine(id, i.ToString()));
				var packet = packets[i];

				// Get packet data
				MemoryStream mem = new MemoryStream();
				BinaryWriter writer = new BinaryWriter(mem);
				packet.WritePacket(writer);
				byte[] data = mem.ToArray();

				if (packet.Iv == null)
					// Generate iv
					packet.Iv = Crypto.GetRandom(Crypto.IV_LENGTH);

				// Write iv and encrypted data
				mem = new MemoryStream();
				writer = new BinaryWriter(mem);
				// Write iv
				writer.Write(packet.Iv);
				// Write encrypted packet data
				writer.Write(Crypto.EncryptSymetric(symmetricKey, packet.Iv, data));
				data = mem.ToArray();

				if (packet.Signature == null)
				{
					if (privateKey != null)
						// Generate signature
						packet.Signature = Crypto.Sign(privateKey, data);
					else
						throw new InvalidOperationException("Found unencrypted packet without private key.");
				}

				// Write signature
				output.Write(packet.Signature);
				// Write iv and encrypted data
				output.Write(data);
			}
		}

		public async Task Load(IIoProvider io)
		{
		}
	}
}
