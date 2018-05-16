using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Responses
{
		
	public class CollaboratorData : CommunicationResponse
	{
		public static async Task<CollaboratorData> Create(LagerBase lager, Collaborator collaborator)
		{
			var result = new CollaboratorData();
			await result.Init(lager, collaborator);
			return result;
		}
		
		CollaboratorData() { }

		async Task Init(LagerBase lager, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
				await lager.Serialiser.Write(output,
					new LagerSerialisationContext(lager),
					collaborator);
			Data = mem.ToArray();
		}

		public async Task<Collaborator> GetCollaborator(LagerBase lager)
		{
			Collaborator result = new Collaborator();
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
				await lager.Serialiser.Read(input,
					new LagerSerialisationContext(lager),
					result);
			return result;
		}
	}
}
