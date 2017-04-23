using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Erwischt;
	using Serialisation;

	public class ErwischtPacket : DataPacket
	{
		protected ErwischtPacket() { }

		public static async Task<ErwischtPacket> Create(Serialiser<LagerClientSerialisationContext> serialiser,
		                    LagerClientSerialisationContext context, ErwischtParticipant erwischtMember, bool isAlive)
		{
			var packet = new ErwischtPacket();
			await packet.Init(serialiser, context, erwischtMember, isAlive);
			return packet;
		}

		async Task Init(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, ErwischtParticipant erwischtMember, bool isAlive)
		{
			var mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				await serialiser.WriteId(output, context, erwischtMember.Game);
				await serialiser.WriteId(output, context, erwischtMember.Member);
				output.Write(isAlive);
			}
			Data = mem.ToArray();
		}

		public override async Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
			{
				ErwischtGame game = await serialiser.ReadFromId<ErwischtGame>(input, context);
				Member member = await serialiser.ReadFromId<Member>(input, context);
				ErwischtParticipant p = context.LagerClient.ErwischtHandler.GetFromIds(game.Id, member.Id);
				bool isAlive = input.ReadBoolean();
				p.IsAlive = isAlive;

				contentString = p.ToString();
			}
		}
	}
}
