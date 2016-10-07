using System;
using System.Collections.Generic;
using System.Linq;
using Zeltlager.UAM;

namespace Zeltlager
{
	[Editable("Zelt")]
	public class Tent : IEditable<Tent>, ISearchable
	{
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

		public Tent() {}

		public Tent(byte number, string name, List<Member> supervisors)
		{
			Number = number;
			Name = name;
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

		public void OnSaveEditing(Tent oldObject)
		{
			if (oldObject != null)
				Lager.CurrentLager.RemoveTent(oldObject);
			Lager.CurrentLager.AddTent(this);
		}

		public Tent CloneDeep()
		{
			return new Tent(Number, Name, new List<Member>(supervisors));
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
