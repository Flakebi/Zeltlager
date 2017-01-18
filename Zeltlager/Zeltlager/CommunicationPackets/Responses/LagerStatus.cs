using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Responses
{
	using Serialisation;
	
	public class LagerStatus : CommunicationResponse
	{
		public static async Task<LagerStatus> Create(LagerBase lager)
		{
			var result = new LagerStatus();
			await result.Init(lager);
			return result;
		}
		
		LagerStatus() { }

		async Task Init(LagerBase lager)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
				await lager.Serialiser.Write(output,
					new LagerSerialisationContext(lager.Manager, lager),
					lager.Status);
			Data = mem.ToArray();
		}

		public async Task ReadRemoteStatus(LagerBase lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
				await lager.Serialiser.Read(input,
					new LagerSerialisationContext(lager.Manager, lager),
					lager.Remote.Status);
		}
	}
}
