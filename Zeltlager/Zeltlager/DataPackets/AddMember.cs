using System.IO;
using System.Linq;

namespace Zeltlager.DataPackets
{
	using Client;

	public class AddMember : DataPacket
	{
		public AddMember() { }

		public AddMember(Member member)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(member.Name);
				output.Write(member.Supervisor);
                output.Write(member.Tent.Id);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				MemberId id = new MemberId();
                id.collaborator = collaborator;
                // Use the next free member id and inrcease the id afterwards.
                id.id = collaborator.NextMemberId++;
				string name = input.ReadString();
				bool supervisor = input.ReadBoolean();
                TentId tentId = input.ReadTentId(lager);

				Tent tent = lager.Tents.First(t => t.Id == tentId);
				lager.AddMember(new Member(id, name, tent, supervisor));
			}
		}
	}
}
