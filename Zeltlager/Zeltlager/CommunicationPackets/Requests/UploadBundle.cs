using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Client;
	using DataPackets;
	using Network;
	using Serialisation;

	/// <summary>
	/// Request the LagerStatus of a lager.
	/// </summary>
	public class UploadBundle : LagerCommunicationRequest
	{
		public static async Task<UploadBundle> Create(LagerClient lager, DataPacketBundle bundle)
		{
			var result = new UploadBundle();
			await result.Init(lager, bundle);
			return result;
		}
		
		UploadBundle() { }

		async Task Init(LagerClient lager, DataPacketBundle bundle)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(bundle.Id);
				await lager.Serialiser.Write(output,
					new LagerSerialisationContext(lager.Manager, lager),
					bundle);
			}
			await CreateData(new CommunicationLagerData(lager, mem.ToArray(), new byte[0]));
		}

		public override async Task Apply(INetworkConnection connection, LagerManager manager)
		{
			// Verify the signatures
			try
			{
				CommunicationLagerData data = await GetData(manager);
				MemoryStream mem = new MemoryStream(data.Unencrypted);
				using (BinaryReader input = new BinaryReader(mem))
				{
					int bundleId = input.ReadInt32();
					// Check if the requested bundle already exists
					if (data.Collaborator.Bundles.ContainsKey(bundleId))
						throw new LagerException("The bundle already exists");
					// Add the bundle
					DataPacketBundle bundle = new DataPacketBundle();
					await data.Lager.Serialiser.Read(input,
						new LagerSerialisationContext(manager, data.Lager),
						bundle);
					await data.Lager.AddBundle(data.Collaborator, bundle);
				}
				await connection.WritePacket(new Responses.Status(true));
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("BundlesRequest", e);
				await connection.WritePacket(new Responses.Status(false));
			}
		}
	}
}
