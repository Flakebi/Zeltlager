using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using DataPackets;
		using UAM;

	[Editable("Wettkampf")]
	public class Competition : Rankable, ISearchable, IDeletable
	{
		LagerClient lager;

		[Editable("Name")]
		public string Name { get; set; }

		public bool IsVisible { get; set; } = true;

		public List<Participant> Participants { get; set; }

		public List<Station> Stations { get; set; }

		public string SearchableText => Name;

		public string SearchableDetail => "";

		Ranking ranking = new Ranking();
		public override Ranking Ranking
		{
			get
			{
				UpdateRanking();
				return ranking;
			}

			set
			{
				throw new NotSupportedException("Setting the ranking of a competition is not supported");
			}
		}

		public Competition() 
		{
			Participants = new List<Participant>();
			Stations = new List<Station>();
		}

		public Competition(PacketId id, string name, LagerClient lager)
		{
			Id = id;
			this.lager = lager;
			Name = name;
			Participants = new List<Participant>();
			Stations = new List<Station>();
		}

		public Competition(LagerClient lager, PacketId id, string name, List<Participant> participants, List<Station> stations)
		{
			this.lager = lager;
			Id = id;
			Name = name;
			Participants = participants;
			Stations = stations;
		}

		public void UpdateRanking()
		{
			if (!Participants.Any())
				return;

			// Create a summed up ranking
			ranking.Results.Clear();
			foreach (var p in Participants)
			{
				int? placePoints = 0;
				bool hasPoints = false;
				foreach (var s in Stations)
				{
					var result = s.Ranking.Results.FirstOrDefault(res => res.Participant == p);
					if (result != null && result.Place.HasValue)
					{
						placePoints += result.Place;
						hasPoints = true;
					}
				}
				if (!hasPoints)
					placePoints = null;
				ranking.AddResult(new CompetitionResult(null, this, p, placePoints, null));
			}
			// Sort and change the place to a better number

			// same points get the same place
			ranking.Results = ranking.Results.Where(r => r.Points != null).ToList();
			if (!ranking.Results.Any())
				return;
			
			ranking.Results.Sort();

			int currentPoints = (int) ranking.Results.First().Points;
			int currentPlace = 1;

			foreach (CompetitionResult cr in ranking.Results)
			{
				if (cr.Points == null)
					continue;
				
				if (cr.Points == currentPoints)
				{
					cr.Place = currentPlace;
				}
				else
				{
					cr.Place = ++currentPlace;
					currentPoints = (int)cr.Points;
				}

				// reset points again for this cr (we only used them for determining the place)
				cr.Points = null;
			}
		}

		public override void AddResult(CompetitionResult cr)
		{
			LagerManager.Log.Error("Competition", "Modifying the ranking of a competition is not supported");
		}

		public void AddStation(Station station) => Stations.Add(station);

		public void RemoveStation(Station station) => Stations.Remove(station);

		public void AddParticipant(Participant participant) => Participants.Add(participant);

		public void RemoveParticipant(Participant participant) => Participants.Remove(participant);

		public override IReadOnlyList<Participant> GetParticipants()
		{
			return Participants;
		}

		public LagerClient GetLagerClient()
		{
			return lager;
		}

		public override Rankable Clone()
		{
			return new Competition(lager, Id?.Clone(), Name, Participants, Stations);
		}

		public override string ToString()
		{
			return string.Format("Competition {0}", Name);
		}
	}
}
