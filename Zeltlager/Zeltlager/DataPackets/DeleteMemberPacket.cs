using System.Linq;

namespace Zeltlager.DataPackets
{
	public class DeleteMemberPacket : DataPacket
	{
		public DeleteMemberPacket() { }

		public DeleteMemberPacket(Member member)
		{
			Data = member.Id.ToBytes();
		}

		public override void Deserialise(Lager lager)
		{
			ushort id = Data.ToUShort(0);
			lager.RemoveMember(lager.Members.First(m => m.Id == id));
		}
	}
}
