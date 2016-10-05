using System;

namespace Zeltlager
{
	[Editable("Teilnehmer")]
	public class Member : IComparable<Member>, IEditable<Member>
	{
		public ushort Id { get; set; }

		[Editable("Name")]
		public string Name { get; set; }
		/// <summary>
		/// The tent in which this member lives, this attribute can not be null.
		/// </summary>
		[Editable("Zelt")]
		public Tent Tent { get; set; }

		[Editable("Betreuer")]
		public bool Supervisor { get; set; }

		public string Display { get { return Name; } }

		public Member() {}

		public Member(ushort id, string name, Tent tent, bool supervisor)
		{
			Id = id;
			Name = name;
			Tent = tent;
			Supervisor = supervisor;
		}

		public override string ToString() => Display;

		public int CompareTo(Member other) => Display.ToLowerInvariant().CompareTo(other.Display.ToLowerInvariant());

		#region Interface implementation

		public void OnSaveEditing(Member oldObject)
		{
			Lager.CurrentLager.RemoveMember(oldObject);
			Lager.CurrentLager.AddMember(this);
		}

		public Member CloneDeep()
		{
			return new Member(Id, Name, Tent, Supervisor);
		}

		#endregion
	}
}
