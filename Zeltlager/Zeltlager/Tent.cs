using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
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
		public int Number { get; set; }

		[Editable("Zeltname")]
		[Serialisation]
		public string Name { get; set; }

		[Editable("Zeltbereuer")]
		[Serialisation(Type = SerialisationType.Reference)]
		List<Member> supervisors;

		[Editable("Mädchenzelt")]
		[Serialisation]
		public bool Girls { get; set; }

		public IReadOnlyList<Member> Supervisors => supervisors;

		public string Display => Number + " " + Name + " " + (Girls ? "♀" : "♂");

		// For deserialisation
		protected static Task<Tent> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.Tents.First(t => t.Id == id));
		}

		public Tent()
		{
			Id = new PacketId(null);
			Number = 0;
			Name = "";
			Girls = false;
			supervisors = new List<Member>();
		}

		// For deserialisetion
		public Tent(LagerClientSerialisationContext context) : this() { }

		public Tent(PacketId id, int number, string name, bool girls, List<Member> supervisors)
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

		// Add the member to a lager after deserialising it
		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			context.LagerClient.AddTent(this);
		}

		// TODO: get members



		#region Interface implementations

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, Tent oldObject)
		{
            DataPacket packet;
			if (oldObject != null)
				packet = await EditPacket.Create(serialiser, context, this);
            else
				packet = await AddPacket.Create(serialiser, context, this);
			await context.LagerClient.AddPacket(packet);
		}

		public Tent Clone()
		{
			return new Tent(Id.Clone(), Number, Name, Girls, new List<Member>(supervisors));
		}

		public string SearchableText => Display;

		public string SearchableDetail => "";

		#endregion

		public override bool Equals(object obj)
		{
			Tent other = obj as Tent;
			if (other == null)
				return false;
			return Number == other.Number && Name == other.Name && Girls == other.Girls &&
				Supervisors.SequenceEqual(other.Supervisors);
		}

		public override int GetHashCode()
		{
			return Number.GetHashCode() ^ Name.GetHashCode() ^ Girls.GetHashCode() ^ Supervisors.GetHashCode();
		}
	}
}
