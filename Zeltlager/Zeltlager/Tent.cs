using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class Tent
	{
		public byte Number { get; set; }
		public string Name { get; set; }
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

		public override string ToString()
		{
			return Number + " " + Name;
		}

		public bool AddSupervisor(Member supervisor)
		{
			if (Supervisors.Contains(supervisor))
				return false;
			supervisors.Add(supervisor);
			return true;
		}

		public bool RemoveSupervisor(Member supervisor)
		{
			return supervisors.Remove(supervisor);
		}
	}
}
