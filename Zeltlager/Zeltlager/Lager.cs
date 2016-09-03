using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zeltlager.DataPackets;

namespace Zeltlager
{
	public class Lager : ILagerPart
	{
		public static bool IsClient { get; set; }
		public static IIoProvider IoProvider { get; set; }

		public static Lager CurrentLager { get; set; }

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
		/// The private key for our own collaborator.
		/// </summary>
		byte[] collaboratorPrivateKey;
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

		byte[] publicKey;
		byte[] privateKey;
		byte[] symmetricKey;

		/// <summary>
		/// The password supplied by the user and used to generate the shared keys.
		/// </summary>
		string password;

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
		/// Creates a new lager.
		/// </summary>
		public void Init()
		{
			// Create the keys for this instance
			salt = Crypto.GetRandom(Crypto.SALT_LENGTH);
			symmetricKey = Crypto.DeriveSymmetricKey(password, salt);
			var keys = Crypto.CreateAsymmetricKeys();
			publicKey = keys.PublicKey;
			privateKey = keys.PrivateKey;

			synchronized = false;
			sentPackets = 0;
			// Create the keys for our own collaborator
			keys = Crypto.CreateAsymmetricKeys();
			collaboratorPrivateKey = keys.PrivateKey;
			Collaborator c = new Collaborator(0, keys.PublicKey);
			ownCollaborator = 0;
		}

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
			// Save packets from collaborators
			await Task.WhenAll(collaborators.Select(async c => await c.Save(rootedIo, symmetricKey)));
		}

		public async Task Load(IIoProvider io)
		{
			string id = Id.ToString();

			var rootedIo = new RootedIoProvider(io, id);
			// Load packets of collaborators
			await Task.WhenAll(collaborators.Select(async c => await c.Load(rootedIo)));
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
