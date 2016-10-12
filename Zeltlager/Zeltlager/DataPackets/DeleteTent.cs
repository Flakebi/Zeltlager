using System.IO;
using System.Linq;

namespace Zeltlager.DataPackets
{
	using Client;

	public class DeleteTent : DataPacket
	{
		public DeleteTent() { }

		public DeleteTent(Tent tent)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
                output.Write(tent.Id);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
                TentId tentId = input.ReadTentId(lager);

				Tent tent = lager.Tents.First(t => t.Id == tentId);
				lager.RemoveTent(tent);
			}
		}
	}
}
