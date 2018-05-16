using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Zeltlager.CommunicationPackets.Responses
{
		
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
			Data = JsonConvert.SerializeObject(lager.Status);
		}

		public async Task ReadRemoteStatus(LagerBase lager)
		{
			lager.Remote.Status = JsonConvert.DeserializeObject<Zeltlager.LagerStatus>(Data);
		}
	}
}
