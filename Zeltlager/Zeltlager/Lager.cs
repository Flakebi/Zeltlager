using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class Lager : ILagerPart
	{
		public static bool IsClient { get; set; }
		public static IIoProvider IoProvider { get; set; }

		public static Lager CurrentLager { get; set; }

		List<Member> members = new List<Member>();
		List<Tent> tents = new List<Tent>();
		List<DataPackets.DataPacket> history = new List<DataPackets.DataPacket>();

		public string Name { get; private set; }
		public IReadOnlyList<Member> Members { get { return members; } }
		public IReadOnlyList<Tent> Tents { get { return tents; } }
		public IReadOnlyList<DataPackets.DataPacket> History { get { return history; } }

		// Subspaces
		public Tournament.Tournament Tournament { get; private set; }
		public Competition.Competition Competition { get; private set; }
		public Erwischt.Erwischt Erwischt { get; private set; }
		public Calendar.Calendar Calendar { get; private set; }

		public Lager(string name)
		{
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
