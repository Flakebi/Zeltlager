using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Zeltlager.DataPackets;

namespace Zeltlager
{
	public class Lager : ILagerPart
	{
		public enum InitStatus
		{
			CreateSymmetricKey,
			CreateGameAsymmetricKey,
			CreateCollaboratorAsymmetricKey,
			Ready
		}

		public static bool IsClient { get; set; }
		public static IIoProvider IoProvider { get; set; }
		public static ICryptoProvider CryptoProvider { get; set; }
		public static Log Log { get; set; }

		public static Client.GlobalSettings ClientGlobalSettings { get; set; }

		public static Lager CurrentLager { get; set; }

		const byte VERSION = 0;
		const string GENERAL_SETTINGS_FILE = "lager.conf";

		static Lager()
		{
			CryptoProvider = new BCCryptoProvider();
			Log = new Log();
		}

		List<Member> members = new List<Member>();
		List<Tent> tents = new List<Tent>();
		List<Collaborator> collaborators = new List<Collaborator>();

		// Data for the client
		/// <summary>
		/// If this instance was synchronized with a server.
		/// </summary>
		bool synchronized;
		/// <summary>
		/// The collaborator that we are.
		/// </summary>
		byte ownCollaborator;
		/// <summary>
		/// The number of packets from our own contributor that were already
		/// sent to the server. This is also the id of the next packet, that
		/// should be sent.
		/// </summary>
		ushort sentPackets;

		// Crypto
		/// <summary>
		/// The salt used for the key derivation functions.
		/// </summary>
		byte[] salt;

		KeyPair asymmetricKey;
		byte[] symmetricKey;

		/// <summary>
		/// The password supplied by the user and used to generate the shared keys.
		/// </summary>
		string password;

		/// <summary>
		/// Packets that could not be loaded.
		/// Each tuple contains the collaborator and the packet id.
		/// </summary>
		public List<Tuple<byte, ushort>> MissingPackets { get; set; }

		public string Name { get; set; }
		public byte Id { get; set; }
		public IReadOnlyList<Member> Members { get { return members; } }
		public IReadOnlyList<Tent> Tents { get { return tents; } }
		public IReadOnlyList<Collaborator> Collaborators { get { return collaborators; } }

		// Subspaces
		public Tournament.Tournament Tournament { get; private set; }
		public Competition.Competition Competition { get; private set; }
		public Erwischt.Erwischt Erwischt { get; private set; }
		public Calendar.Calendar Calendar { get; private set; }

		public Lager(byte id, string name, string password)
		{
			Id = id;
			Name = name;
			this.password = password;

			MissingPackets = new List<Tuple<byte, ushort>>();

			Tournament = new Tournament.Tournament(this);
			Competition = new Competition.Competition(this);
			Erwischt = new Erwischt.Erwischt(this);
			Calendar = new Calendar.Calendar(this);
		}

		ILagerPart[] GetParts()
		{
			return new ILagerPart[]
			{
				this,
				Tournament,
				Competition,
				Erwischt,
				Calendar,
			};
		}

		/// <summary>
		/// Assemble the packet history from all collaborators.
		/// This orders the packets in chronological order and removes packet bundles.
		/// </summary>
		/// <returns>The flat history of packets.</returns>
		List<DataPacket> GetHistory()
		{
			// Use OrderBy which is a stable sorting algorithm.
			return collaborators.SelectMany(col => col.Packets).SelectMany(packet =>
				{
					// Flatten bundles here
					return new DataPacket[] { packet };
				}).OrderBy(packet => packet.Timestamp).ToList();
		}

		/// <summary>
		/// Apply the whole history.
		/// </summary>
		/// <returns>
		/// Returns true if the whole history could be applied
		/// successful, false if an error occured.
		/// </returns>
		public bool ApplyHistory()
		{
			List<DataPacket> history = GetHistory();
			bool success = true;
			foreach (var packet in history)
			{
				try
				{
					packet.Deserialise(this);
				}
				catch (Exception e)
				{
					// Log the exception
					Log.Exception("Lager", e);
					success = false;
				}
			}
			return success;
		}

		/// <summary>
		/// Adds a packet to our own collaborator and applies the packet.
		/// </summary>
		public async Task AddPacket(DataPacket packet)
		{
			var collaborator = collaborators[ownCollaborator];
			collaborator.AddPacket(packet);
			// First, write the packet to disk (it will be serialised in that process)
			await collaborator.SavePacket(IoProvider, symmetricKey, (ushort)(collaborator.Packets.Count - 1));

			// Save the updated packet count
			await SaveGeneralSettings();

			// Then deserialise it to apply it
			packet.Deserialise(this);
		}

		/// <summary>
		/// Creates a new lager.
		/// </summary>
		public async Task Init(Action<InitStatus> statusUpdate)
		{
			// Create the keys for this instance
			statusUpdate(InitStatus.CreateSymmetricKey);
			salt = await CryptoProvider.GetRandom(CryptoConstants.SALT_LENGTH);
			symmetricKey = await CryptoProvider.DeriveSymmetricKey(password, salt);
			statusUpdate(InitStatus.CreateGameAsymmetricKey);
			asymmetricKey = await CryptoProvider.CreateAsymmetricKeys();

			synchronized = false;
			sentPackets = 0;
			// Create the keys for our own collaborator
			statusUpdate(InitStatus.CreateCollaboratorAsymmetricKey);
			var keys = await CryptoProvider.CreateAsymmetricKeys();
			Collaborator c = new Collaborator(0, keys.Modulus, keys.PublicKey, keys.PrivateKey);
			collaborators.Add(c);
			ownCollaborator = 0;
			statusUpdate(InitStatus.Ready);
		}

		public Task Save() => Save(IoProvider);

		/// <summary>
		/// Stores this instance on the filesystem.
		/// </summary>
		/// <param name="io">The used io system.</param>
		/// <returns>If the save was successful.</returns>
		public async Task Save(IIoProvider io)
		{
			string id = Id.ToString();
			if (!await io.ExistsFolder(id))
				await io.CreateFolder(id);
			var rootedIo = new RootedIoProvider(io, id);

			await SaveGeneralSettings(rootedIo);

			// Save packets from collaborators
			await Task.WhenAll(collaborators.Select(async c => await c.SaveAll(rootedIo, symmetricKey)));
		}

		Task SaveGeneralSettings() => SaveGeneralSettings(new RootedIoProvider(IoProvider, Id.ToString()));

		async Task SaveGeneralSettings(IIoProvider io)
		{
			using (BinaryWriter output = new BinaryWriter(await io.WriteFile(GENERAL_SETTINGS_FILE)))
			{
				byte[] iv = await CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);
				output.Write(VERSION);
				output.Write(asymmetricKey.Modulus);
				output.Write(salt);
				output.Write(iv);

				// Encrypt the rest
				MemoryStream mem = new MemoryStream();
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					if (IsClient)
					{
						writer.Write(synchronized);
						writer.Write(ownCollaborator);
						writer.Write(sentPackets);
						writer.Write(collaborators[ownCollaborator].PrivateKey);
					}
					writer.Write(asymmetricKey.PrivateKey);
					// Write collaborators
					writer.Write((byte)collaborators.Count);
					for (int i = 0; i < collaborators.Count; i++)
					{
						writer.Write(collaborators[i].Modulus);
						writer.Write((ushort)collaborators[i].Packets.Count);
					}
				}
				output.Write(await CryptoProvider.EncryptSymetric(symmetricKey, iv, mem.ToArray()));
			}
		}

		public Task<bool> Load() => Load(IoProvider);

		/// <summary>
		/// Load the collaborators and packets of this lager.
		/// </summary>
		/// <param name="io">The io provider.</param>
		/// <returns>True if everything could be loaded and applied successfully, false otherwise.</returns>
		public async Task<bool> Load(IIoProvider io)
		{
			string id = Id.ToString();
			var rootedIo = new RootedIoProvider(io, id);

			byte[] ownCollaboratorPrivateKey = null;
			ushort[] collaboratorPacketCounts;
			// Load general settings
			using (BinaryReader input = new BinaryReader(await rootedIo.ReadFile(GENERAL_SETTINGS_FILE)))
			{
				if (input.ReadByte() == VERSION)
				{
					var modulus = input.ReadBytes(CryptoConstants.MODULUS_LENGTH);
					salt = input.ReadBytes(CryptoConstants.SALT_LENGTH);
					var iv = input.ReadBytes(CryptoConstants.IV_LENGTH);

					// The rest is encrypted
					symmetricKey = await CryptoProvider.DeriveSymmetricKey(password, salt);
					int length = await CryptoProvider.GetSymmetricEncryptedLength((int)(input.BaseStream.Length - input.BaseStream.Position));
					using (MemoryStream mem = new MemoryStream(await CryptoProvider.DecryptSymetric(symmetricKey, iv, input.ReadBytes(length))))
					{
						using (BinaryReader reader = new BinaryReader(mem))
						{
							if (IsClient)
							{
								synchronized = reader.ReadBoolean();
								ownCollaborator = reader.ReadByte();
								sentPackets = reader.ReadUInt16();
								ownCollaboratorPrivateKey = reader.ReadBytes(CryptoConstants.PRIVATE_KEY_LENGTH);
							}

							var privateKey = reader.ReadBytes(CryptoConstants.PRIVATE_KEY_LENGTH);
							asymmetricKey = new KeyPair(modulus, CryptoConstants.DEFAULT_PUBLIC_KEY, privateKey);

							// Read collaborators
							byte collaboratorCount = reader.ReadByte();
							collaboratorPacketCounts = new ushort[collaboratorCount];
							collaborators.Capacity = collaboratorCount;
							for (byte i = 0; i < collaboratorCount; i++)
							{
								var collaboratorModulus = reader.ReadBytes(CryptoConstants.MODULUS_LENGTH);
								collaboratorPacketCounts[i] = reader.ReadUInt16();
								if (i != ownCollaborator && IsClient)
									collaborators.Add(new Collaborator(i, collaboratorModulus, CryptoConstants.DEFAULT_PUBLIC_KEY));
								else
									collaborators.Add(new Collaborator(i, collaboratorModulus, CryptoConstants.DEFAULT_PUBLIC_KEY, ownCollaboratorPrivateKey));
							}
						}
					}
				}
				else
					throw new Exception("Can't read lager with unknown version.");
			}

			// Load packets of collaborators
			bool success = (await Task.WhenAll(collaborators.Select(async (c, i) => await c.Load(rootedIo, symmetricKey, this, VERSION, collaboratorPacketCounts[i])))).All(b => b);

			success &= ApplyHistory();
			return success;
		}

		public void AddMember(Member member)
		{
			if (Members.Any(m => m.Id == member.Id))
				throw new InvalidOperationException("A member with this id exists already.");
			members.Add(member);
		}

		public void RemoveMember(Member member)
		{
			if (!members.Remove(member))
				throw new InvalidOperationException("A member with this id wasn't found for deletion.");
		}

		public void AddTent(Tent tent)
		{
			if (Tents.Any(t => t.Number == tent.Number))
				throw new InvalidOperationException("A tent with this number exists already.");
			tents.Add(tent);
		}

		public void RemoveTent(Tent tent)
		{
			if (!tents.Remove(tent))
				throw new InvalidOperationException("A tent with this number wasn't found for deletion.");
		}
	}
}
