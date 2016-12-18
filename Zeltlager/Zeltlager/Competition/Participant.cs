using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Serialisation;
	using UAM;
	
	/// <summary>
	/// represents a participant in a comptetion, could be a tent, a mixed group or a single person
	/// </summary>
	[Editable("Teilnehmer")]
	public class Participant : ISearchable, IEditable<Participant>
	{
		// TODO Participants serialisieren

		Competition competition;

		[Editable("Name")]
		string Name { get; set; }

		public Participant(string name, Competition competition)
		{
			this.Name = name;
			this.competition = competition;
		}

		#region Interface implementation

		public string SearchableText => Name;

		public string SearchableDetail => "";

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, Participant oldObject)
		{
			// TODO: Packets
		}

		public Participant Clone()
		{
			return new Participant(Name, competition);
		}

		#endregion
	}
}
