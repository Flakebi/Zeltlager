using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using UAM;
    using Serialisation;

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
			Lager = lager;
			Name = name;
		}

		public void AddStation(Station station) => Stations.Add(station);

		public void RemoveStation(Station station) => Stations.Remove(station);

		public void AddParticipant(Participant participant) => Participants.Add(participant);

		public void RemoveParticipant(Participant participant) => Participants.Remove(participant);

		#region Interface implementation

		public Task OnSaveEditing(
            Serialiser<LagerClientSerialisationContext> serialiser,
            LagerClientSerialisationContext context, Competition oldObject)
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
