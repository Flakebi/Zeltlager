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
		public List<Member> Supervisors { get; set; }

		public string Display { get { return Number + " " + Name; } }

		public Tent()
		{
		}

		public Tent(byte number, string name)
		{
			Number = number;
			Name = name;
		}

		public override string ToString()
		{
			return Number + " " + Name;
		}
	}
}
