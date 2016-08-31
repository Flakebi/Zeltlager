using System;
using System.Collections.Generic;
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
			Tent tent = new Tent(1, "Regenbogenforellen");
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
			string id = Id.ToString();
			if (!await io.ExistsFolder(id))
				await io.CreateFolder(id);

			var rootedIo = new RoutedIoProvider(io, id);
			// Save packets from collaborators
			await Task.WhenAll(collaborators.Select(c => c.Save(rootedIo)));

			return true;
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
