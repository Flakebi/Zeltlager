using System;

namespace Zeltlager
{
	public class Member : IComparable<Member>
	{
		public ushort Id { get; set; }
		public string Name { get; set; }
		/// <summary>
		/// The tent in which this member lives, this attribute can be null.
		/// </summary>
		public Tent Tent { get; set; }
		public bool Supervisor { get; set; }

		public string Display { get { return Name; } }

		public Member()
		{
		}

		public Member(ushort id, string name, Tent tent, bool supervisor)
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
