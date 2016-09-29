using System.Linq;

namespace Zeltlager.DataPackets
{
	public class DeleteMember : DataPacket
	{
		public DeleteMember() { }

		public DeleteMember(Member member)
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
