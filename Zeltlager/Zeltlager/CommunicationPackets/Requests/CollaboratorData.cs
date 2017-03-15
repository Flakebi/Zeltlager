using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Client;
	using Cryptography;
	using Network;

	/// <summary>
	/// Request the data of a collaborator.
	/// </summary>
	public class CollaboratorData : LagerCommunicationRequest
	{
		public static async Task<CollaboratorData> Create(LagerClient lager, KeyPair collaborator)
		{
			var result = new CollaboratorData();
			await result.Init(lager, collaborator);
			return result;
		}
		
		CollaboratorData() { }

		async Task Init(LagerClient lager, KeyPair collaborator)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
				output.WritePublicKey(collaborator);
			await CreateData(new CommunicationLagerData(lager, mem.ToArray(), new byte[0]));
		}

		public override async Task Apply(INetworkConnection connection, LagerManager manager)
		{
			// Verify the signatures
			try
			{
				CommunicationLagerData data = await GetData(manager);
				// Get the collaborator
				KeyPair collaborator;
				MemoryStream mem = new MemoryStream(data.Unencrypted);
				using (BinaryReader input = new BinaryReader(mem))
					collaborator = input.ReadPublicKey();
				if (!data.Lager.Status.BundleCount.Any(c => c.Item1 == collaborator))
					throw new LagerException("Unknown collaborator data requested");
				// Send the collaborator data
				await connection.WritePacket(await Responses.CollaboratorData.Create(data.Lager, data.Lager.Collaborators[collaborator]));
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("CollaboratorDataRequest", e);
				await connection.WritePacket(new Responses.Status(false));
			}
		}
	}
}
