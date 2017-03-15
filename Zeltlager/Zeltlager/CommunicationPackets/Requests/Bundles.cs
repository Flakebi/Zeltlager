using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Client;
	using Network;

	/// <summary>
	/// Request the LagerStatus of a lager.
	/// </summary>
	public class Bundles : LagerCommunicationRequest
	{
		public static async Task<Bundles> Create(LagerClient lager, IEnumerable<Tuple<Collaborator, int>> requestedBundles)
		{
			var result = new Bundles();
			await result.Init(lager, requestedBundles);
			return result;
		}
		
		Bundles() { }

		async Task Init(LagerClient lager, IEnumerable<Tuple<Collaborator, int>> requestedBundles)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(requestedBundles.Count());
				foreach (var b in requestedBundles)
				{
					// Write collaborator and bundle id
					output.Write(lager.Remote.Status.GetCollaboratorId(b.Item1));
					output.Write(b.Item2);
				}
			}
			await CreateData(new CommunicationLagerData(lager, mem.ToArray(), new byte[0]));
		}

		public override async Task Apply(INetworkConnection connection, LagerManager manager)
		{
			// Verify the signatures
			try
			{
				CommunicationLagerData data = await GetData(manager);
				// Send the requested bundles
				var packets = new List<CommunicationPacket>();
				MemoryStream mem = new MemoryStream(data.Unencrypted);
				using (BinaryReader input = new BinaryReader(mem))
				{
					int bundleCount = input.ReadInt32();
					for (int i = 0; i < bundleCount; i++)
					{
						int collaboratorId = input.ReadInt32();
						if (collaboratorId < 0 || collaboratorId >= data.Lager.Status.BundleCount.Count)
							throw new LagerException("Unknown collaborator");
						Collaborator collaborator = data.Lager.Collaborators[data.Lager.Status.BundleCount[collaboratorId].Item1];
						int bundleId = input.ReadInt32();
						if (bundleId >= collaborator.Bundles.Count)
							throw new LagerException("Unknown bundle");
						packets.Add(await Responses.Bundle.Create(data.Lager, collaborator, collaborator.Bundles[bundleId]));
					}
				}
				await connection.WritePackets(packets);
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("BundlesRequest", e);
				await connection.WritePacket(new Responses.Status(false));
			}
		}
	}
}
