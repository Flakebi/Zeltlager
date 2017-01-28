using System.IO;

namespace Zeltlager.CommunicationPackets.Responses
{
	public class AddLager : CommunicationResponse
	{
		AddLager() { }
		
		public AddLager(int id)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
				output.Write(id);
			Data = mem.ToArray();
		}

		public int GetRemoteId()
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
				return input.ReadInt32();
		}
	}
}
