using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class Member : IComparable<Member>
	{
		public uint Id { get; set; }
		public string Name { get; set; }
		public Tent Tent { get; set; }
		public bool Supervisor { get; set; }

		public string Display { get { return Name; } }

		public Member()
		{
		}

		public Member(uint id, string name, Tent tent, bool supervisor)
		{
			Id = id;
			Name = name;
			Tent = tent;
			Supervisor = supervisor;
		}

		public override string ToString()
		{
			return Name;
		}

		public int CompareTo(Member other)
		{
			return Display.ToLowerInvariant().CompareTo(other.Display.ToLowerInvariant());
		}
	}
}
