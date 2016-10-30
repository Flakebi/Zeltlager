using System;
using System.Linq;
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
		public PacketId Id { get; set; }

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

		public string Display => Name + (Supervisor ? " \ud83d\ude0e" : "");

		// For deserialisation
		protected static Task<Member> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.Members.First(m => m.Id == id));
		}

		public Member()
		{
			Id = new PacketId(null);
			Name = "";
			Supervisor = false;
		}

		// For deserialisetion
		public Member(LagerClientSerialisationContext context) : this()
		{ }

		public Member(PacketId id, string name, Tent tent, bool supervisor)
		{
			Id = id;
			Name = name;
			Tent = tent;
			Supervisor = supervisor;
		}

		// Add the member to a lager after deserialising it
		protected void Add(LagerClientSerialisationContext context)
		{
			// Reset the collaborator in the id to prevent spoofing
			Id = Id.Clone(context.PacketId.Creator);
			context.LagerClient.AddMember(this);
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
