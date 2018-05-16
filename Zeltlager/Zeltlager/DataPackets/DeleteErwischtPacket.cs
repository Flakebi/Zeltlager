using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Erwischt;
	
	public class DeleteErwischtPacket : DataPacket
	{
		protected DeleteErwischtPacket() { }

		public static async Task<DeleteErwischtPacket> Create(ErwischtGame game)
		{
			var packet = new DeleteErwischtPacket();
			await packet.Init(serialiser, context, game);
			return packet;
		}

		async Task Init(ErwischtGame game)
		{
			var mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				await serialiser.WriteId(output, context, game);
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise()
		{
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
			{
				ErwischtGame game = await serialiser.ReadFromId<ErwischtGame>(input, context);
				game.IsVisible = false;

				contentString = game.Name;
			}
		}
	}
}
