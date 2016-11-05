using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	public class AddCollaborator : DataPacket
	{
		protected AddCollaborator() { }

		public static async Task<AddCollaborator> Create(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, Collaborator collaborator)
		{
			var packet = new AddCollaborator();
			await packet.Init(serialiser, context, collaborator);
			return packet;
		}

		async Task Init(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				await serialiser.Write(output, context, collaborator);
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			Collaborator collaborator = new Collaborator();
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
				await serialiser.Read(input, context, collaborator);
			// Search the right collaborator
			collaborator = context.Lager.Collaborators[collaborator.Key];
			// Add the read collaborator to the list
			context.PacketId.Creator.Collaborators[context.PacketId] = collaborator;
		}
	}
}
