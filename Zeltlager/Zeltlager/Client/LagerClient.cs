using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	using DataPackets;
	using Serialisation;

	public class LagerClient : LagerBase
	{
		public enum InitStatus
		{
			CreateSymmetricKey,
			CreateGameAsymmetricKey,
			CreateCollaboratorAsymmetricKey,
			Ready
		}

		public static GlobalSettings ClientGlobalSettings { get; set; }

		public string Name { get; set; }
		public IReadOnlyList<Member> Members { get { return members; } }
		public IReadOnlyList<Tent> Tents { get { return tents; } }

		// Subspaces
		public Competition.CompetitionHandler CompetitionHandler { get; private set; }
		public Erwischt.Erwischt Erwischt { get; private set; }
		public Calendar.Calendar Calendar { get; private set; }

		List<Member> members = new List<Member>();
		List<Tent> tents = new List<Tent>();

		/// <summary>
		/// The collaborator that we are.
		/// </summary>
		Collaborator ownCollaborator;
		/// <summary>
		/// Bundles that were created but not yet sent to the server.
		/// </summary>
		List<DataPacketBundle> newBundles = new List<DataPacketBundle>();

		// Crypto
		public byte[] SymmetricKey { get; private set; }

		/// <summary>
		/// The password supplied by the user and used to generate the shared keys.
		/// </summary>
		string password;

		public LagerClient(byte id, string name, string password)
		{
			Id = id;
			Name = name;
			this.password = password;

			MissingPackets = new List<Tuple<byte, ushort>>();

			CompetitionHandler = new Competition.CompetitionHandler(this);
			Erwischt = new Erwischt.Erwischt(this);
			Calendar = new Calendar.Calendar();
		}

		/// <summary>
		/// Assemble the packet history from all collaborators.
		/// This orders the packets in chronological order and removes packet bundles.
		/// </summary>
		/// <returns>The flat history of packets.</returns>
		async Task<List<Tuple<PacketId, DataPacket>>> GetHistory()
		{
			var packets = await Task.WhenAll(collaborators
				.Select(async col =>
				{
					var packets2 = await Task.WhenAll(col.Bundles.Select(b =>
						b.GetPackets(new LagerClientSerialisationContext(this))));
					return packets2.SelectMany(p => p);
				}));
			// Use OrderBy which is a stable sorting algorithm
			return packets.SelectMany(p => p)
				.OrderBy(packet => packet.Item2.Timestamp).ToList();
		}

		/// <summary>
		/// Apply the whole history.
		/// </summary>
		/// <returns>
		/// Returns true if the whole history was applied successfully,
		/// false if an error occured.
		/// </returns>
		public async Task<bool> ApplyHistory()
		{
			List<Tuple<PacketId, DataPacket>> history = await GetHistory();
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(this);
			bool success = true;
			foreach (var packet in history)
			{
				try
				{
					context.PacketId = packet.Item1;
					packet.Item2.Deserialise(context);
				} catch (Exception e)
				{
					// Log the exception
					await Log.Exception("Lager", e);
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
			DataPacketBundle bundle;
			// Check if a usable bundle exists
			if (!newBundles.Any() || newBundles.Last().Packets.Count == DataPacketBundle.MAX_PACKETS)
			{
				bundle = new DataPacketBundle();
				newBundles.Add(bundle);
			} else
				bundle = newBundles.Last();
			bundle.AddPacket(packet);

			// First, write the packet to disk
			var rootedIo = new RootedIoProvider(ioProvider, Id.ToString());
			//TODO Save newPackets
			//await ownCollaborator.SavePacket(rootedIo, SymmetricKey, (ushort)(ownCollaborator.Bundles.Count - 1));

			// Save the updated packet count
			await SaveGeneralSettings();

			// Then deserialise it to apply it
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(this);
			context.PacketId = new PacketId(ownCollaborator, bundle, (byte)(bundle.Packets.Count - 1));
			packet.Deserialise(context);
		}

		/// <summary>
		/// Creates a new lager.
		/// </summary>
		public async Task Init(Action<InitStatus> statusUpdate)
		{
			// Create the keys for this instance
			statusUpdate(InitStatus.CreateSymmetricKey);
			salt = await CryptoProvider.GetRandom(CryptoConstants.SALT_LENGTH);
			SymmetricKey = await CryptoProvider.DeriveSymmetricKey(password, salt);
			statusUpdate(InitStatus.CreateGameAsymmetricKey);
			asymmetricKey = await CryptoProvider.CreateAsymmetricKey();

			// Create the keys for our own collaborator
			statusUpdate(InitStatus.CreateCollaboratorAsymmetricKey);
			var key = await CryptoProvider.CreateAsymmetricKey();
			Collaborator c = new Collaborator(0, key);
			collaborators.Add(c);
			ownCollaborator = c;
			statusUpdate(InitStatus.Ready);
		}

		public Task Save() => Save(ioProvider);

		/// <summary>
		/// Stores this instance on the filesystem.
		/// </summary>
		/// <param name="io">The used io system.</param>
		/// <returns>If the save was successful.</returns>
		public async Task Save(IIoProvider io)
		{
			//TODO Is this method needed?
			string id = Id.ToString();
			if (!await io.ExistsFolder(id))
				await io.CreateFolder(id);
			var rootedIo = new RootedIoProvider(io, id);

			await SaveGeneralSettings(rootedIo);

			// Save packets from collaborators
			await Task.WhenAll(collaborators.Select(async c => await c.SaveAll(rootedIo, SymmetricKey)));

			//TODO Save newPackets
		}

		Task SaveGeneralSettings() => SaveGeneralSettings(new RootedIoProvider(ioProvider, Id.ToString()));

		async Task SaveGeneralSettings(IIoProvider io)
		{
			using (BinaryWriter output = new BinaryWriter(await io.WriteFile(GENERAL_SETTINGS_FILE)))
			{
				byte[] iv = await CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);
				output.Write(VERSION);
				output.WritePublicKey(AsymmetricKey);
				output.Write(salt);
				output.Write(iv);

				// Encrypt the rest
				MemoryStream mem = new MemoryStream();
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					if (IsClient)
					{
						writer.Write(ownCollaborator.Id);
						writer.WritePrivateKey(ownCollaborator.Key);
					}
					writer.WritePrivateKey(AsymmetricKey);
					//TODO Write status

					// Write collaborator keys
					writer.Write((byte)collaborators.Count);
					for (int i = 0; i < collaborators.Count; i++)
					{
						writer.WritePublicKey(collaborators[i].Key);
						writer.Write((ushort)collaborators[i].Bundles.Count);
					}
				}
				output.Write(await CryptoProvider.EncryptSymetric(SymmetricKey, iv, mem.ToArray()));
			}
		}

		public Task<bool> Load() => Load(ioProvider);

		/// <summary>
		/// Load the collaborators and packets of this lager.
		/// </summary>
		/// <param name="io">The io provider.</param>
		/// <returns>True if everything could be loaded and applied successfully, false otherwise.</returns>
		public async Task<bool> Load(IIoProvider io)
		{
			string id = Id.ToString();
			var rootedIo = new RootedIoProvider(io, id);

			ushort[] collaboratorPacketCounts;
			// Load general settings
			using (BinaryReader input = new BinaryReader(await rootedIo.ReadFile(GENERAL_SETTINGS_FILE)))
			{
				if (input.ReadByte() == VERSION)
				{
					asymmetricKey = input.ReadPublicKey();
					salt = input.ReadBytes(CryptoConstants.SALT_LENGTH);
					var iv = input.ReadBytes(CryptoConstants.IV_LENGTH);

					// The rest is encrypted
					SymmetricKey = await CryptoProvider.DeriveSymmetricKey(password, salt);
					int length = await CryptoProvider.GetSymmetricEncryptedLength((int)(input.BaseStream.Length - input.BaseStream.Position));
					using (MemoryStream mem = new MemoryStream(await CryptoProvider.DecryptSymetric(SymmetricKey, iv, input.ReadBytes(length))))
					{
						using (BinaryReader reader = new BinaryReader(mem))
						{
							KeyPair ownCollaboratorPrivateKey = new KeyPair(null, null, null);
							byte ownCollaboratorId = 0;
							if (IsClient)
							{
								ownCollaboratorId = reader.ReadByte();
								ownCollaboratorPrivateKey = reader.ReadPrivateKey();
							}

							asymmetricKey = reader.ReadPrivateKey();

							// Read collaborators
							byte collaboratorCount = reader.ReadByte();
							collaboratorPacketCounts = new ushort[collaboratorCount];
							collaborators.Capacity = collaboratorCount;
							for (byte i = 0; i < collaboratorCount; i++)
							{
								KeyPair collaboratorPublicKey = reader.ReadPublicKey();
								collaboratorPacketCounts[i] = reader.ReadUInt16();
								if (i == ownCollaboratorId && IsClient)
								{
									ownCollaborator = new Collaborator(i, ownCollaboratorPrivateKey);
									collaborators.Add(ownCollaborator);
								} else
									collaborators.Add(new Collaborator(i, collaboratorPublicKey));
							}
						}
					}
				} else
					throw new Exception("Can't read lager with unknown version.");
			}

			// Load packets of collaborators
			//TODO Load packets
			bool success = false;// (await Task.WhenAll(collaborators.Select(async (c, i) => await c.Load(rootedIo, SymmetricKey, this, VERSION, collaboratorPacketCounts[i])))).All(b => b);

			success &= await ApplyHistory();
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
			if (Tents.Any(t => t.Id == tent.Id))
				throw new InvalidOperationException("A tent with this id exists already.");
			tents.Add(tent);
		}

		public void RemoveTent(Tent tent)
		{
			if (!tents.Remove(tent))
				throw new InvalidOperationException("A tent with this id wasn't found for deletion.");
		}

		public Tent GetTentFromDisplay(string display)
		{
			Tent t = null;
			// find correct tent from display string
			foreach (Tent tent in Tents)
			{
				if (tent.Display == display)
				{
					t = tent;
					break;
				}
			}
			return t;
		}
	}
}
