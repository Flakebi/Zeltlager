using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zeltlager
{
	[Editable("Zelt")]
	public class Tent : IEditable<Tent>
	{
		[Editable("Zeltnummer")]
		public byte Number { get; set; }

		[Editable("Zeltname")]
		public string Name { get; set; }

		[Editable("Zeltbereuer")]
		List<Member> supervisors = new List<Member>();

		public IReadOnlyList<Member> Supervisors { get { return supervisors; } }

		public string Display { get { return Number + " " + Name; } }

		public Tent()
		{
		}

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
			Lager.CurrentLager.RemoveTent(oldObject);
			Lager.CurrentLager.AddTent(this);
		}

		public Tent CloneDeep()
		{
			return new Tent(Number, Name, new List<Member>(supervisors));
		}

		#endregion
	}
}
