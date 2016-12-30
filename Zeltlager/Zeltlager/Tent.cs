using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Client;
	using DataPackets;
	using Serialisation;
	using UAM;

	[Editable("Zelt")]
	public class Tent : Editable<Tent>, ISearchable
	{
		LagerClient lager;

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Zeltnummer")]
		[Serialisation]
		public int Number { get; set; }

		[Editable("Zeltname")]
		[Serialisation]
		public string Name { get; set; }

		[Serialisation(Type = SerialisationType.Reference)]
		List<Member> supervisors;

		[Editable("Mädchenzelt")]
		[Serialisation]
		public bool Girls { get; set; }

		[Editable("Zeltbetreuer")]
		public List<Member> Supervisors => supervisors;

		public IReadOnlyList<Member> SupervisorsList => lager.Supervisors;
		public string Display => Number + " " + Name + " " + (Girls ? "♀" : "♂");

		public string DisplayDetail 
		{
			get
			{
				if (supervisors.Any())
				{
					return GetMembers().Count + " Teilnehmer, " + (supervisors.First().Name);
				}
				return GetMembers().Count + " Teilnehmer";
			}
		}

		// For deserialisation
		protected static Task<Tent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.Tents.First(t => t.Id == id));
		}

		public Tent()
		{
			Id = new PacketId(null);
			Number = 0;
			Name = "";
			Girls = false;
			supervisors = new List<Member>();
		}

		// For deserialisation
		public Tent(LagerClientSerialisationContext context) : this() { }

		public Tent(PacketId id, int number, string name, bool girls, List<Member> supervisors, LagerClient lager)
		{
			Id = id;
			Number = number;
			Name = name;
			Girls = girls;
			this.supervisors = supervisors;
			this.lager = lager;
		}

		public override string ToString() => Display;

		public bool AddSupervisor(Member supervisor)
		{
			if (Supervisors.Contains(supervisor))
				return false;
			supervisors.Add(supervisor);
			return true;
		}

		public bool RemoveSupervisor(Member supervisor) => supervisors.Remove(supervisor);

		public List<Member> GetMembers()
		{
			return new List<Member>(lager.Members.Where((arg) => arg.Tent.Equals(this)));
		}

		// Add the member to a lager after deserialising it
		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			this.lager = context.LagerClient;
			context.LagerClient.AddTent(this);
		}


		#region Interface implementations

		public override Tent Clone()
		{
			return new Tent(Id?.Clone(), Number, Name, Girls, new List<Member>(supervisors), lager);
		}

		public string SearchableText => Display;

		public string SearchableDetail => DisplayDetail;

		#endregion

		public override bool Equals(object obj)
		{
			Tent other = obj as Tent;
			if (other == null)
				return false;
			return Number == other.Number && Name == other.Name && Girls == other.Girls;
		}

		public override int GetHashCode()
		{
			return Number.GetHashCode() ^ Name.GetHashCode() ^ Girls.GetHashCode() ^ Supervisors.GetHashCode();
		}

		public static Tent GetFromString(LagerClient lager, string str)
		{
			return lager.GetTentFromDisplay(str);
		}
	}
}
