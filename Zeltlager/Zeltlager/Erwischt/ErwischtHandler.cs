using System;
using System.Collections.Generic;
using Zeltlager.Client;
using System.Linq;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;

namespace Zeltlager.Erwischt
{
	/// <summary>
	/// Handles all different Erwischt games.
	/// </summary>
	public class ErwischtHandler
	{
		LagerClient lager;
		public List<ErwischtGame> Games { get; private set; }
		public List<ErwischtGame> VisibleGames => Games.Where(g => g.IsVisible).ToList();
		public ErwischtGame CurrentGame { get; set; }

		public ErwischtHandler(LagerClient lager)
		{
			this.lager = lager;
			Games = new List<ErwischtGame>();
			CurrentGame = null;
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
