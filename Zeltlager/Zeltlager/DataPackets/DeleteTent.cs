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
				tent.Id.Write(output);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				TentId tentId = new TentId(lager, input);

				Tent tent = lager.Tents.First(t => t.Id == tentId);
				lager.RemoveTent(tent);
			}
		}
	}
}
