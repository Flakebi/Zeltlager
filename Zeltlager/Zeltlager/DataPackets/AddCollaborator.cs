using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	public class AddCollaborator : DataPacket
	{
		public AddCollaborator() { }

		public async Task Init(LagerClientSerialisationContext context, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				await context.LagerClient.ClientSerialiser.Write(output, context, collaborator);
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise(LagerClientSerialisationContext context)
		{
			Collaborator collaborator = new Collaborator();
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
				await context.LagerClient.ClientSerialiser.Read(input, context, collaborator);
			// Search the right collaborator
			collaborator = context.Lager.Collaborators.First(c => c.Key.PublicKey.SequenceEqual(collaborator.Key.PublicKey));
			// Add the read collaborator to the list
			context.PacketId.Creator.Collaborators[context.PacketId] = collaborator;
		}
	}
}
