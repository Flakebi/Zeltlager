using System;
using Zeltlager.UAM;

namespace Zeltlager
{
	using Client;

	[Editable("Teilnehmer")]
	public class Member : IComparable<Member>, IEditable<Member>, ISearchable
	{
		public MemberId Id { get; set; }

		[Editable("Name")]
		public string Name { get; set; }
		/// <summary>
		/// The tent in which this member lives, this attribute can not be null.
		/// </summary>
		[Editable("Zelt")]
		public Tent Tent { get; set; }

		[Editable("Betreuer")]
		public bool Supervisor { get; set; }

		public string Display { get { return Name + " " + (Supervisor ? "\ud83d\ude0e" : ""); } }

		public Member() {}

		public Member(MemberId id, string name, Tent tent, bool supervisor)
		{
			Id = id;
			Name = name;
			Tent = tent;
			Supervisor = supervisor;
		}

		public override string ToString() => Display;

		public int CompareTo(Member other) => string.Compare(Display, other.Display, StringComparison.OrdinalIgnoreCase);

		#region Interface implementation

		public void OnSaveEditing(Member oldObject)
		{
			if (oldObject != null)
				LagerClient.CurrentLager.RemoveMember(oldObject);
			LagerClient.CurrentLager.AddMember(this);
		}

		public Member CloneDeep()
		{
			return new Member(Id, Name, Tent, Supervisor);
		}

		public string SearchableText
		{
			get { return Display; }
		}

		public string SearchableDetail
		{
			get { return Tent.Display; }
		}

		#endregion
	}
}
