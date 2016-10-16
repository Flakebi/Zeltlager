using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zeltlager.UAM;

namespace Zeltlager.Competition
{
	/// <summary>
	/// represents one station in the competition and the results achieved there
	/// </summary>
	public class Station : ISearchable, IEditable<Station>
	{
		Competition competition;

		[Editable("Name")]
		string name;
		List<CompetitionResult> results;

		public Station(string name, Competition competition)
		{
			this.name = name;
			this.competition = competition;
			results = new List<CompetitionResult>();
		}

		private Station(string name, Competition competition, List<CompetitionResult> results) : this(name, competition)
		{
			this.results = results;
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
