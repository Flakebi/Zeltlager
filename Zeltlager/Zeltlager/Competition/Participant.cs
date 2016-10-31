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
		Competition competition;

		[Editable("Name")]
		string name;

		public Participant(string name, Competition competition)
		{
			this.name = name;
			this.competition = competition;
		}

		#region Interface implementation

		public string SearchableText => name;

		public string SearchableDetail => "";

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, Participant oldObject)
		{
			// TODO: Packets
		}

		public Participant Clone()
		{
			return new Participant(name, competition);
		}

		#endregion
	}
}
