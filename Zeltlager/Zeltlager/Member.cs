using System;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Client;
	using DataPackets;
    using Serialisation;
	using UAM;

	[Editable("Teilnehmer")]
	public class Member : IComparable<Member>, IEditable<Member>, ISearchable
    {
        [Serialisation(Type = SerialisationType.Id)]
        public MemberId Id { get; set; }

		[Editable("Name")]
        [Serialisation]
		public string Name { get; set; }
		/// <summary>
		/// The tent in which this member lives, this attribute can not be null.
		/// </summary>
		[Editable("Zelt")]
        [Serialisation(Type = SerialisationType.Reference)]
        public Tent Tent { get; set; }

		[Editable("Betreuer")]
        [Serialisation]
        public bool Supervisor { get; set; }

		public string Display { get { return Name + (Supervisor ? " \ud83d\ude0e" : ""); } }

		public Member()
		{
			Id = new MemberId();
			Name = "";
			Supervisor = false;
		}

		public Member(MemberId id, string name, Tent tent, bool supervisor)
		{
			Id = id;
			Name = name;
			Tent = tent;
			Supervisor = supervisor;
		}

        // Constructors for deserialisation
        protected Member(SerialisationContext context, string name, Tent tent, bool supervisor)
        {
            // Use the next free member id and inrcease the id afterwards.
            Id = new MemberId(context.Collaborator, context.Collaborator.NextMemberId++);
            Name = name;
            Tent = tent;
            Supervisor = supervisor;
        }

		public override string ToString() => Display;

		public int CompareTo(Member other) => string.Compare(Display, other.Display, StringComparison.OrdinalIgnoreCase);

		#region Interface implementation

		public async Task OnSaveEditing(Member oldObject, LagerClient lager)
		{
			if (oldObject != null)
				await lager.AddPacket(new DeleteMember(oldObject));
			await lager.AddPacket(new AddMember(this));
		}

		public Member Clone()
		{
			return new Member(Id.Clone(), Name, Tent, Supervisor);
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
