using System.IO;
using System.Linq;

namespace Zeltlager.DataPackets
{
	using Client;

	public class DeleteMember : DataPacket
	{
		public DeleteMember() { }

		public DeleteMember(Member member)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				member.Id.Write(output);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				MemberId id = new MemberId(lager, input);
				lager.RemoveMember(lager.Members.First(m => m.Id == id));
			}
		}
	}
}
