using System.Collections.Generic;

namespace Zeltlager.Competition
{
	using System;
	using System.Threading.Tasks;
	using Client;
	using UAM;

	public class Competition : IEditable<Competition>, ISearchable
	{
		LagerClient lager;
		string name;
		List<Participant> participants;

		public Competition(LagerClient lager, string name)
		{
			this.lager = lager;
			this.name = name;
		}



		#region Interface implementation

		public Task OnSaveEditing(Competition oldObject)
		{
			// TODO: durch packets ersetzen
			if (oldObject != null)
			{
				LagerClient.CurrentLager.CompetitionHandler.RemoveCompetition(oldObject);
			}
			LagerClient.CurrentLager.CompetitionHandler.AddCompetition(this);
			return null;
		}

		public Competition CloneDeep()
		{
			return new Competition(lager, name);
		}

		public string SearchableText { get { return name; } }

		public string SearchableDetail { get { return ""; } }

		#endregion
	}
}
