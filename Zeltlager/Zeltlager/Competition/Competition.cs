using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using DataPackets;
	using Serialisation;
	using UAM;

	[Editable("Wettkampf")]
	public class Competition : Rankable, ISearchable, IDeletable
	{
		LagerClient lager;

		[Editable("Name")]
		[Serialisation]
		public string Name { get; set; }

		[Serialisation]
		public bool IsVisible { get; set; } = true;

		public List<Participant> Participants { get; set; }

		public List<Station> Stations { get; set; }

		public string SearchableText => Name;

		public string SearchableDetail => "";

		public override Ranking Ranking
		{
			get
			{
				// Create a sumed up ranking
				Ranking r = new Ranking();
				foreach (var p in Participants)
				{
					int placePoints = 0;
					foreach (var s in Stations)
					{
						var result = s.Ranking.Results.First(res => res.Participant == p);
						placePoints += result.Place ?? 0;
					}
					r.AddResult(new CompetitionResult(null, this, p, null, placePoints));
				}
				// Sort and change the place to a better number
				r.Results.Sort();
				for (int i = 0; i < r.Results.Count; i++)
					r.Results[i].Place = i + 1;
				return r;
			}

			set
			{
				throw new NotSupportedException("Setting the ranking of a competition is not supported");
			}
		}

		protected static Task<Competition> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetCompetitionFromPacketId(id));
		}

		public Competition() 
		{
			Participants = new List<Participant>();
			Stations = new List<Station>();
		}

		public Competition(LagerClientSerialisationContext context) : this() {}

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

		public override void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			context.LagerClient.CompetitionHandler.AddCompetition(this);
		}

		public override void AddResult(CompetitionResult cr)
		{
			throw new NotSupportedException("Modifying the ranking of a competition is not supported");
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
			return new Competition(lager, Id, Name, Participants, Stations);
		}
	}
}
