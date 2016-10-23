using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Client;
	using DataPackets;
	using Serialisation;
	using UAM;

	[Editable("Zelt")]
	public class Tent : IEditable<Tent>, ISearchable
	{
		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Zeltnummer")]
		[Serialisation]
		public byte Number { get; set; }

		[Editable("Zeltname")]
		[Serialisation]
		public string Name { get; set; }

		[Editable("Zeltbereuer")]
		[Serialisation(Type = SerialisationType.Reference)]
		List<Member> supervisors;

		[Editable("Mädchenzelt")]
		[Serialisation]
		public bool Girls { get; set; }

		public IReadOnlyList<Member> Supervisors { get { return supervisors; } }

		public string Display { get { return Number + " " + Name + " " + (Girls ? "♀" : "♂"); } }

		// For deserialisation
		protected static Task<Tent> GetFromId(LagerSerialisationContext context, PacketId id)
		{
			return ((LagerClientSerialisationContext)context).LagerClient.Tents.First(t => t.Id == id);
		}

		public Tent()
		{
			Id = new PacketId();
			Number = 0;
			Name = "";
			Girls = false;
			supervisors = new List<Member>();
		}

		public Tent(PacketId id, byte number, string name, bool girls, List<Member> supervisors)
		{
			Id = id;
			Number = number;
			Name = name;
			Girls = girls;
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

		// TODO: get members



		#region Interface implementations

		public async Task OnSaveEditing(Tent oldObject, LagerClient lager)
		{
			if (oldObject != null)
				await lager.AddPacket(new DeleteTent(oldObject));
			await lager.AddPacket(new AddTent(this));
		}

		public Tent Clone()
		{
			return new Tent(Id.CloneShallow(), Number, Name, Girls, new List<Member>(supervisors));
		}

		public string SearchableText
		{
			get { return Display; }
		}

		public string SearchableDetail
		{
			get { return ""; }
		}

		#endregion
	}
}
