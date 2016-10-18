using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zeltlager.Client;
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

		public string SearchableText { get { return name; } }

		public string SearchableDetail { get { return ""; } }

		public Task OnSaveEditing(Station oldObject, LagerClient lager)
		{
			// TODO: Packets
			return null;
		}

		public Station Clone()
		{
			return new Station(name, competition, ranking);
		}



		#endregion
	}
}
