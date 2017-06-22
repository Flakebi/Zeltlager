using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using Serialisation;

	public class Ranking
	{
		[Serialisation]
		public List<CompetitionResult> Results { get; set; }

		public Ranking()
		{
			Results = new List<CompetitionResult>();
		}

		public void AddResult(CompetitionResult cr)
		{
			Results.Add(cr);
		}

		public void RemoveResult(Participant p)
		{
			// TODO wie tun wir das?
		}

		public async Task Rank(bool increasing)
		{
			var results = Results.Where(r => r.Points.HasValue);
			if (!results.Any())
				return;
			
			IEnumerable<CompetitionResult> x;
			if (!increasing)
				x = results.OrderByDescending(r => r.Points);
			else
				x = results.OrderBy(r => r.Points); // vorne die kleinen Werte hinten die großen

			// same points should get the same place
			CompetitionResult[] crs = x.ToArray();
			int currentPoints = (int) crs[0].Points;
			int currentPlace = 1;

			foreach (CompetitionResult cr in crs)
			{
				bool changedSomething = false;
				if (cr.Points == currentPoints)
				{
					if (cr.Place != currentPlace)
					{
						cr.Place = currentPlace;
						changedSomething = true;
					}
				}
				else
				{
					++currentPlace;
					currentPoints = (int) cr.Points;
					if (cr.Place != currentPlace)
					{
						cr.Place = currentPlace;
						changedSomething = true;
					}
				}

				if (changedSomething)
				{
					LagerClient lager = cr.Participant.GetLagerClient();
					LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
					context.PacketId = cr.Id;
					await lager.AddPacket(await DataPackets.EditPacket.Create(lager.ClientSerialiser, context, cr));	
				}
			}
		}
		}
	}
