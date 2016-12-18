using System.IO;
using System.Collections.Generic;

namespace Zeltlager.CommunicationPackets.Responses
{
	public class LagerData
	{
	}

	public class ListLagers : CommunicationResponse
	{
		public ListLagers(LagerManager manager)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(manager.Lagers.Count);
				foreach (var pair in manager.Lagers)
				{
					output.Write(pair.Key);
					output.Write(pair.Value.Data.Length);
					output.Write(pair.Value.Data);
				}
			}
			Data = mem.ToArray();
		}

		public Dictionary<int, byte[]> GetLagerData()
		{
			Dictionary<int, byte[]> result = new Dictionary<int, byte[]>();
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				int count = input.ReadInt32();
				for (int i = 0; i < count; i++)
				{
					int id = input.ReadInt32();
					int length = input.ReadInt32();
					byte[] data = input.ReadBytes(length);
					result.Add(id, data);
				}
			}
			return result;
		}
	}
}
