using System.Collections.Generic;

namespace Zeltlager.Competition
{
	using System.Threading.Tasks;
	using Client;
	using UAM;

	[Editable("Wettkampf")]
	public class Competition : IEditable<Competition>, ISearchable
	{
		public LagerClient Lager;

		[Editable("Name")]
		public string Name;
		public List<Participant> Participants;
		public List<Station> Stations;

		public Competition(LagerClient lager, string name)
		{
			this.Lager = lager;
			this.Name = name;
		}

		public void AddStation(Station station) { Stations.Add(station); }

		public void RemoveStation(Station station) { Stations.Remove(station); }

		public void AddParticipant(Participant participant) { Participants.Add(participant); }

		public void RemoveParticipant(Participant participant) { Participants.Remove(participant); }

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

		public Competition Clone()
		{
			return new Competition(Lager, Name);
		}

		public string SearchableText { get { return Name; } }

		public string SearchableDetail { get { return ""; } }

		#endregion
	}
}
