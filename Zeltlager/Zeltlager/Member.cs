using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Zeltlager.Client;

namespace Zeltlager
{
	using DataPackets;
	using Serialisation;
	using UAM;

	[Editable("Teilnehmer")]
	public class Member : Editable<Member>, IComparable<Member>, ISearchable, IDeletable
	{
		LagerClient lager;

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Name")]
		[Serialisation]
		public string Name { get; set; }

		// The tent in which this member lives, this attribute can not be null.
		[Editable("Zelt")]
		[Serialisation(Type = SerialisationType.Reference)]
		public Tent Tent { get; set; }

		public IReadOnlyList<Tent> TentList => lager.VisibleTents;

		[Editable("Betreuer")]
		[Serialisation]
		public bool Supervisor { get; set; }

		public string Display => Name + (Supervisor ? " \ud83d\ude0e" : "");

		// For deserialisation
		protected static Task<Member> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.Members.First(m => m.Id == id));
		}

		public Member(LagerClient lager)
		{
			Id = new PacketId(null);
			Name = "";
			Supervisor = false;
			this.lager = lager;
		}

		// For deserialisation
		public Member(LagerClientSerialisationContext context) : this(context.LagerClient) {}

		public Member(PacketId id, string name, Tent tent, bool supervisor, LagerClient lager)
		{
			Id = id;
			Name = name;
			Tent = tent;
			Supervisor = supervisor;
			this.lager = lager;
		}

		// Add the member to a lager after deserialising it
		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			context.LagerClient.AddMember(this);
		}

		public override string ToString() => Display;

		public int CompareTo(Member other) => string.Compare(Display, other.Display, StringComparison.OrdinalIgnoreCase);

		#region Interface implementation

		public override Member Clone()
		{
			return new Member(Id?.Clone(), Name, Tent, Supervisor, lager);
		}

		public string SearchableText => Display;

		public string SearchableDetail => Tent.Display;

		[Serialisation]
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
