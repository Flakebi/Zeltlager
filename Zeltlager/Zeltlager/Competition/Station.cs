using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zeltlager.UAM;

namespace Zeltlager.Competition
{
	/// <summary>
	/// represents one station in the competition and the results achieved there
	/// </summary>
	[Editable("Station")]
	public class Station : ISearchable, IEditable<Station>
	{
		Competition competition;

		[Editable("Name")]
		string name;

		Dictionary<Participant, CompetitionResult> results;

		public Station(string name, Competition competition)
		{
			this.name = name;
			this.competition = competition;
			results = new Dictionary<Participant, CompetitionResult>();
		}

		Station(string name, Competition competition, Dictionary<Participant, CompetitionResult> results) : this(name, competition)
		{
			this.results = results;
		}

		public void AddResult(Participant participant, int points, int? place)
		{
			results[participant] = new CompetitionResult(points, place);
		}

		#region Interface implementation

		public string SearchableText { get { return name; } }

		public string SearchableDetail { get { return ""; } }

		public Task OnSaveEditing(Station oldObject)
		{
			// TODO: Packets
			return null;
		}

		public Station Clone()
		{
			return new Station(name, competition, results);
		}



		#endregion
	}
}
