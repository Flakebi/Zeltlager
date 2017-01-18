using System;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Client;
	using Network;

	/// <summary>
	/// Request the LagerStatus of a lager.
	/// </summary>
	public class LagerStatus : LagerCommunicationRequest
	{
		public static async Task<LagerStatus> Create(LagerClient lager)
		{
			var result = new LagerStatus();
			await result.Init(lager);
			return result;
		}
		
		LagerStatus() { }

		async Task Init(LagerClient lager)
		{
			await CreateData(lager, new byte[0], new byte[0]);
		}

		public override async Task Apply(INetworkConnection connection, LagerManager manager)
		{
			// Verify the signatures
			try
			{
				await GetData(manager);
				// Send the lager status
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("LagerStatusRequest", e);
				await connection.WritePacket(new Responses.Status(false));
			}
		}
	}
}
