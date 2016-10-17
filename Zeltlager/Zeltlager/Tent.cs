using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Client;
	using DataPackets;
	using UAM;

	[Editable("Zelt")]
	public class Tent : IEditable<Tent>, ISearchable
	{
		public TentId Id { get; set; }

		[Editable("Zeltnummer")]
		public byte Number { get; set; }

		[Editable("Zeltname")]
		public string Name { get; set; }

		[Editable("Zeltbereuer")]
		List<Member> supervisors;

		[Editable("Mädchenzelt")]
		public bool Girls { get; set; }

		public IReadOnlyList<Member> Supervisors { get { return supervisors; } }

		public string Display { get { return Number + " " + Name + " " + (Girls ? "♀" : "♂"); } }

		public Tent()
		{
			Id = new TentId();
			Number = 0;
			Name = "";
			Girls = false;
			supervisors = new List<Member>();
		}

		public Tent(TentId id, byte number, string name, bool girls, List<Member> supervisors)
		{
			Id = id;
			Number = number;
			Name = name;
			Girls = girls;
			this.supervisors = supervisors;
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

		// TODO: get members



		#region Interface implementations

		public async Task OnSaveEditing(Tent oldObject, LagerClient lager)
		{
			if (oldObject != null)
				await lager.AddPacket(new DeleteTent(oldObject));
			await lager.AddPacket(new AddTent(this));
		}

		public Tent Clone()
		{
			return new Tent(Id.CloneShallow(), Number, Name, Girls, new List<Member>(supervisors));
		}

		public string SearchableText
		{
			get { return Display; }
		}

		public string SearchableDetail
		{
			get { return ""; }
		}

		#endregion
	}
}
