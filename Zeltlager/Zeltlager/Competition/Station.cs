using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Serialisation;
	using UAM;
	
	/// <summary>
	/// represents one station in the competition and the results achieved there
	/// </summary>
	[Editable("Station")]
	public class Station : ISearchable, IEditable<Station>
	{
		Competition competition;

		[Editable("Name")]
		string name;
		Member supervisor;
		Ranking ranking;

		public Station(string name, Competition competition)
		{
			this.name = name;
			this.competition = competition;
			ranking =  new Ranking();
		}

		Station(string name, Competition competition, Ranking ranking) : this(name, competition)
		{
			this.ranking = ranking;
		}

		public void AddResult(Participant participant, int? points = null, int? place = null)
		{
			
		}

		#region Interface implementation

		public string SearchableText => name;

		public string SearchableDetail => "";

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, Station oldObject)
		{
			// TODO: Packets
		}

		public Station Clone()
		{
			return new Station(name, competition, ranking);
		}

		#endregion
	}
}
