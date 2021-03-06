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
					new LagerSerialisationContext(lager),
					bundle);
			}
			await CreateData(new CommunicationLagerData(lager, mem.ToArray(), new byte[0]));
		}

		public override async Task Apply(INetworkConnection connection, LagerManager manager)
		{
			// Verify the signatures
			int bundleId = -1;
			Collaborator col = null;
			try
			{
				CommunicationLagerData data = await GetData(manager);
				col = data.Collaborator;
				MemoryStream mem = new MemoryStream(data.Unencrypted);
				using (BinaryReader input = new BinaryReader(mem))
				{
					bundleId = input.ReadInt32();
					// Check if the requested bundle already exists
					if (bundleId < data.Collaborator.Bundles.Count)
						throw new LagerException("The bundle already exists");
					// Add the bundle
					DataPacketBundle bundle = new DataPacketBundle();
					await data.Lager.Serialiser.Read(input,
						new LagerSerialisationContext(data.Lager),
						bundle);
					bundle.Id = bundleId;
					await bundle.Verify(data.Collaborator);
					await data.Lager.AddBundle(data.Collaborator, bundle);
				}
				await connection.WritePacket(new Responses.Status(true));
				await LagerManager.Log.Info("Upload bundle for lager " + data.Lager.Id, "Uploaded " + bundleId + " for " + data.Collaborator);
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("BundlesRequest for bundle " + bundleId + " from " + (col.ToString() ?? "unknown"), e);
				await connection.WritePacket(new Responses.Status(false));
			}
		}
	}
}
