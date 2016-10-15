using System.Collections.Generic;
using System.Linq;
using Zeltlager.UAM;
using System.Threading.Tasks;
using Zeltlager.DataPackets;

namespace Zeltlager
{
	using Client;

	[Editable("Zelt")]
	public class Tent : IEditable<Tent>, ISearchable
	{
		public TentId Id { get; set; }

		[Editable("Zeltnummer")]
		public byte Number { get; set; }

		[Editable("Zeltname")]
		public string Name { get; set; }

		[Editable("Zeltbereuer")]
		List<Member> supervisors = new List<Member>();

		[Editable("Mädchenzelt")]
		public bool Girls { get; set; }

		public IReadOnlyList<Member> Supervisors { get { return supervisors; } }

		public string Display { get { return Number + " " + Name + " " + (Girls ? "♀" : "♂"); } }

		public Tent() { }

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

		#region Interface implementations

		public async Task OnSaveEditing(Tent oldObject)
		{
			if (oldObject != null)
				await LagerClient.CurrentLager.AddPacket(new DeleteTent(oldObject));
			await LagerClient.CurrentLager.AddPacket(new AddTent(this));
		}

		public Tent CloneDeep()
		{
			return new Tent(Id, Number, Name, Girls, new List<Member>(supervisors));
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
