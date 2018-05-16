using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Zeltlager.DataPackets
{
	using Erwischt;
	using Zeltlager.Client;

	public class ErwischtPacket : DataPacket
	{
		public PacketId GameId { get; private set; }
		public PacketId MemberId { get; private set; }
		public bool IsAlive { get; private set; }

		protected ErwischtPacket() { }

		public static async Task<ErwischtPacket> Create(ErwischtParticipant erwischtMember, bool isAlive)
		{
			var packet = new ErwischtPacket()
			{
				GameId = erwischtMember.Game.Id,
				MemberId = erwischtMember.Member.Id,
				IsAlive = isAlive,
			};
			return packet;
		}

		public override async Task Deserialise(LagerClient lager)
		{
			ErwischtGame game = lager.ErwischtHandler.Games.Find(eg => eg.Id == GameId);
			Member member = lager.Members.First(m => m.Id == MemberId);
			ErwischtParticipant p = lager.ErwischtHandler.GetFromIds(game.Id, member.Id);
			p.IsAlive = IsAlive;

			contentString = p.ToString();
		}
	}
}
