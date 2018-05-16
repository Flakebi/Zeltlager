using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using System;
	using Client;
	using DataPackets;
	using Newtonsoft.Json;
		using UAM;

	[Editable("Zelt")]
	public class Tent : Editable<Tent>, ISearchable, IDeletable
	{
		LagerClient lager;

		[JsonIgnore]
		public PacketId Id { get; set; }

		[Editable("Zeltnummer")]
		public int Number { get; set; }

		[Editable("Zeltname")]
		public string Name { get; set; }

		List<Member> supervisors;

		[Editable("Mädchenzelt")]
		public bool Girls { get; set; }

		[Editable("Zeltbetreuer")]
		// todo json reference
		public List<Member> Supervisors => supervisors;

		[JsonIgnore]
		public IReadOnlyList<Member> SupervisorsList => lager.VisibleSupervisors;

		[JsonIgnore]
		public string Display => Number + " " + Name + " " + (Girls ? "♀" : "♂");

		[JsonIgnore]
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
		public Tent()
		{
			Id = new PacketId(null);
			Number = 0;
			Name = "";
			Girls = false;
			supervisors = new List<Member>();
		}

		public Tent(PacketId id, int number, string name, bool girls, List<Member> supervisors, LagerClient lager)
		{
			Id = id;
			Number = number;
			Name = name;
			Girls = girls;

			if (supervisors == null)
				this.supervisors = new List<Member>();
			else
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
			return new List<Member>(lager.VisibleMembers.Where((arg) => arg.Tent.Equals(this)));
		}

		#region Interface implementations

		public override Tent Clone()
		{
			return new Tent(Id?.Clone(), Number, Name, Girls, new List<Member>(supervisors), lager);
		}

		[JsonIgnore]
		public string SearchableText => Display;

		[JsonIgnore]
		public string SearchableDetail => DisplayDetail;

		public bool IsVisible { get; set; } = true;

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
