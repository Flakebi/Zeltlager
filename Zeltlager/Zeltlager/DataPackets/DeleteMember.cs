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
                output.Write(member.Id);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				MemberId id = input.ReadMemberId(lager);
				lager.RemoveMember(lager.Members.First(m => m.Id == id));
			}
		}
	}
}
