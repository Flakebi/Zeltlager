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
		bool synchronized = false;
		/// <summary>
		/// The collaborator that we are.
		/// </summary>
		byte ownCollaborator;
		/// <summary>
		/// The private key for our own collaborator.
		/// </summary>
		byte[] collaboratorPrivateKey;

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

		public Lager(byte id, string name)
		{
			Id = id;
			Name = name;

			Tournament = new Tournament.Tournament(this);
			Competition = new Competition.Competition(this);
			Erwischt = new Erwischt.Erwischt(this);
			Calendar = new Calendar.Calendar(this);

			//TODO remove debug code
			Tent tent = new Tent(1, "Regenbogenforellen", new List<Member>());
			tents.Add(tent);
			members.Add(new Member(0, "Caro", tent, true));
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
		/// Stores this instance on the filesystem.
		/// </summary>
		/// <param name="io">The used io system.</param>
		/// <returns>If the save was successful.</returns>
		public async Task<bool> Save(IIoProvider io)
		{
			try
			{
				string id = Id.ToString();
				if (!await io.ExistsFolder(id))
					await io.CreateFolder(id);

				var rootedIo = new RoutedIoProvider(io, id);
				// Save packets from collaborators
				return (await Task.WhenAll(collaborators.Select(async c => await c.Save(rootedIo)))).All(b => b);
			}
			catch (IOException e)
			{
				return false;
			}
		}

		public async Task<bool> Load(IIoProvider io)
		{
			try
			{
				string id = Id.ToString();

				var rootedIo = new RoutedIoProvider(io, id);
				// Load packets of collaborators
				return (await Task.WhenAll(collaborators.Select(async c => await c.Load(rootedIo)))).All(b => b);
			}
			catch (IOException e)
			{
				return false;
			}
		}

		public bool AddMember(Member member)
		{
			if (Members.Any(m => m.Id == member.Id))
				// A member with this id exists already.
				return false;
			members.Add(member);
			return true;
		}

		public bool RemoveMember(Member member)
		{
			// A member with this id wasn't found for deletion.
			return members.Remove(member);
		}

		public bool AddTent(Tent tent)
		{
			if (Tents.Any(t => t.Number == tent.Number))
				// A tent with this number exists already.
				return false;
			tents.Add(tent);
			return true;
		}

		public bool RemoveTent(Tent tent)
		{
			// A tent with this number wasn't found for deletion.
			return tents.Remove(tent);
		}
	}
}
