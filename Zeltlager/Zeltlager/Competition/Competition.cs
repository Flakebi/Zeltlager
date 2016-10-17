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
		public Ranking Ranking;


		public Competition(LagerClient lager, string name)
		{
			this.Lager = lager;
			this.Name = name;
		}

		public void AddStation(Station station) => Stations.Add(station);

		public void RemoveStation(Station station) => Stations.Remove(station);

		public void AddParticipant(Participant participant) => Participants.Add(participant);

		public void RemoveParticipant(Participant participant) => Participants.Remove(participant);

		#region Interface implementation

		public Task OnSaveEditing(Competition oldObject, LagerClient lager)
		{
			// TODO: durch packets ersetzen
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
