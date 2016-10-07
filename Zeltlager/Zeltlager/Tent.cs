﻿using System.Collections.Generic;
using System.Linq;

namespace Zeltlager
{
	using Client;

	[Editable("Zelt")]
	public class Tent : IEditable<Tent>
	{
		[Editable("Zeltnummer")]
		public TentId Id { get; set; }

		[Editable("Zeltname")]
		public string Name { get; set; }

		[Editable("Zeltbereuer")]
		List<Member> supervisors = new List<Member>();

		public IReadOnlyList<Member> Supervisors { get { return supervisors; } }

		public string Display { get { return Id + " " + Name; } }

		public Tent()
		{
		}

		public Tent(TentId id, string name, List<Member> supervisors)
		{
			Id = id;
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
			LagerClient.CurrentLager.RemoveTent(oldObject);
			LagerClient.CurrentLager.AddTent(this);
		}

		public Tent CloneDeep()
		{
			return new Tent(Id, Name, new List<Member>(supervisors));
		}

		#endregion
	}
}
