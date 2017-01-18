using System.IO;

namespace Zeltlager.CommunicationPackets.Responses
{
	public class Status : CommunicationResponse
	{
		Status() { }
		
		public Status(bool success)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
				output.Write(success);
			Data = mem.ToArray();
		}

		public bool GetSuccess()
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
				return input.ReadBoolean();
		}
	}
}
