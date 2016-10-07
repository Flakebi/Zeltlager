using System.IO;
using System.Linq;

namespace Zeltlager.DataPackets
{
	using Client;

	public class AddSupervisorToTent : DataPacket
	{
		public AddSupervisorToTent() { }

		public AddSupervisorToTent(Member supervisor, Tent tent)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				tent.Id.Write(output);
				supervisor.Id.Write(output);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				TentId tentId = new TentId(lager, input);
				MemberId id = new MemberId(lager, input);

				Member supervisor = lager.Members.First(m => m.Id == id);
				Tent tent = lager.Tents.First(t => t.Id == tentId);
				tent.AddSupervisor(supervisor);
			}
		}
	}
}
