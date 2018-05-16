using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Zeltlager.Client;

namespace Zeltlager
{
	using DataPackets;
	using Newtonsoft.Json;
		using UAM;

	[Editable("Teilnehmer")]
	public class Member : Editable<Member>, IComparable<Member>, ISearchable, IDeletable
	{
		LagerClient lager;

		[JsonIgnore]
		public PacketId Id { get; set; }

		[Editable("Name")]
		public string Name { get; set; }

		// The tent in which this member lives, this attribute can not be null.
		[Editable("Zelt")]
		// todo json refernce
		public Tent Tent { get; set; }

		[JsonIgnore]
		public IReadOnlyList<Tent> TentList => lager.VisibleTents;

		[Editable("Betreuer")]
		public bool Supervisor { get; set; }

		[JsonIgnore]
		public string Display => Name + (Supervisor ? " \ud83d\ude0e" : "");


		public Member(LagerClient lager)
		{
			Id = new PacketId(null);
			Name = "";
			Supervisor = false;
			this.lager = lager;
		}

		public Member(PacketId id, string name, Tent tent, bool supervisor, LagerClient lager)
		{
			Id = id;
			Name = name;
			Tent = tent;
			Supervisor = supervisor;
			this.lager = lager;
		}

		public override string ToString() => Display;

		public int CompareTo(Member other) => string.Compare(Display, other.Display, StringComparison.OrdinalIgnoreCase);

		#region Interface implementation

		public override Member Clone()
		{
			return new Member(Id?.Clone(), Name, Tent, Supervisor, lager);
		}

		[JsonIgnore]
		public string SearchableText => Display;

		[JsonIgnore]
		public string SearchableDetail => Tent.Display;

		[JsonIgnore]
		public bool IsVisible { get; set; } = true;

		#endregion

		public override bool Equals(object obj)
		{
			Member other = obj as Member;
			if (other == null)
				return false;
			return Name == other.Name && Supervisor == other.Supervisor && Tent.Equals(other.Tent);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode() ^ Supervisor.GetHashCode() ^ Tent.GetHashCode();
		}

		public static Member GetFromString(LagerClient lager, string str)
		{
			return lager.GetMemberFromString(str);
		}
	}
}
