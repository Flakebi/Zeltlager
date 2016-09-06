using System.Linq;

namespace Zeltlager.DataPackets
{
	public class DeleteMemberPacket : DataPacket
	{
		ushort id;

		public DeleteMemberPacket() { }

		public DeleteMemberPacket(Member member)
		{
			id = member.Id;
		}

		public override void Serialise()
		{
			Data = id.ToBytes();
		}

		public override void Deserialise(Lager lager)
		{
			id = Data.ToUShort(0);
			lager.RemoveMember(lager.Members.First(m => m.Id == id));
		}
	}
}
