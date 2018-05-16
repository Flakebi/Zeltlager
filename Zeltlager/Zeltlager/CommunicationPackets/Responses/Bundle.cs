using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Zeltlager.CommunicationPackets.Responses
{
	using DataPackets;
		
	public class Bundle : CommunicationResponse
	{
		public static async Task<Bundle> Create(LagerBase lager, Collaborator collaborator, DataPacketBundle bundle)
		{
			var result = new Bundle();
			await result.Init(lager, collaborator, bundle);
			return result;
		}
		
		Bundle() { }

		async Task Init(LagerBase lager, Collaborator collaborator, DataPacketBundle bundle)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				// Write collaborator and bundle id
				output.Write(lager.Status.GetCollaboratorId(collaborator));
				output.Write(bundle.Id);
				await lager.Serialiser.Write(output,
					new LagerSerialisationContext(lager),
					bundle);
			}
			Data = mem.ToArray();
		}

		public async Task ReadBundle(LagerBase lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				int collaboratorId = input.ReadInt32();
				if (collaboratorId < 0 || collaboratorId >= lager.Status.BundleCount.Count)
					throw new LagerException("Received bundle for unknown collaborator");
				Collaborator collaborator = lager.Collaborators[lager.Remote.Status.BundleCount[collaboratorId].Item1];
				int bundleId = input.ReadInt32();
				DataPacketBundle bundle = JsonConvert.DeserializeObject<DataPacketBundle>(input);
					new DataPacketBundle();
				bundle.Id = bundleId;

				// Check if the bundle does already exist
				if (bundleId < collaborator.Bundles.Count)
					throw new LagerException("Received an already existing bundle");
				await bundle.Verify(collaborator);
				await lager.AddBundle(collaborator, bundle);
			}
		}
	}
}
