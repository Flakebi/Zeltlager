using System.IO;
using System.Linq;

namespace Zeltlager.DataPackets
{
	using Client;

	public class AddMember : DataPacket
	{
		public AddMember() { }

		public AddMember(Member member)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				member.Id.Write(output);
				output.Write(member.Name);
				output.Write(member.Supervisor);
				member.Tent.Id.Write(output);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				MemberId id = new MemberId(lager, input);
				string name = input.ReadString();
				bool supervisor = input.ReadBoolean();
				TentId tentId = new TentId(lager, input);

				Tent tent = lager.Tents.First(t => t.Id == tentId);
				lager.AddMember(new Member(id, name, tent, supervisor));
			}
		}
	}
}
