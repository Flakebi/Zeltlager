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
			IEnumerable<CompetitionResult> x;
			if (!increasing)
				x = results.OrderByDescending(r => r.Points);
			else
				x = results.OrderBy(r => r.Points);

			CompetitionResult[] crs = x.ToArray();
			for (int i = 0; i < crs.Length; i++)
			{
				var cr = crs[i];
				if (cr.Place != i + 1)
				{
					cr.Place = i + 1;
					LagerClient lager = cr.Participant.GetLagerClient();
					LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager.Manager, lager);
					context.PacketId = cr.Id;
					await lager.AddPacket(await DataPackets.EditPacket.Create(lager.ClientSerialiser, context, cr));
				}
			}
		}
	}
}
