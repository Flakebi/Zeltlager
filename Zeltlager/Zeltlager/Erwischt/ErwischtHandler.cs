using System.Collections.Generic;
using System.Linq;

namespace Zeltlager.Erwischt
{
	using Client;
	using DataPackets;

	/// <summary>
	/// Handles all different Erwischt games.
	/// </summary>
	public class ErwischtHandler
	{
		LagerClient lager;
		public List<ErwischtGame> Games { get; private set; }
		public List<ErwischtGame> VisibleGames => Games.Where(g => g.IsVisible).ToList();

		public ErwischtHandler(LagerClient lager)
		{
			this.lager = lager;
			Games = new List<ErwischtGame>();
		}

		public ErwischtParticipant GetFromIds(PacketId gameId, PacketId memberId)
		{
			return Games.First(g => g.Id == gameId).ErwischtParticipants.First(ep => ep.Member.Id == memberId);
		}

		public ErwischtGame GetNewestGame()
		{
			VisibleGames.Sort((x, y) => y.Id.Packet.Timestamp.CompareTo(x.Id.Packet.Timestamp));
			return VisibleGames.Last();
		}
	}
}
