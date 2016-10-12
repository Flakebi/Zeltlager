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
                output.Write(tent.Id);
                output.Write(supervisor.Id);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				TentId tentId = input.ReadTentId(lager);
                MemberId id = input.ReadMemberId(lager);

				Member supervisor = lager.Members.First(m => m.Id == id);
				Tent tent = lager.Tents.First(t => t.Id == tentId);
				tent.AddSupervisor(supervisor);
			}
		}
	}
}
