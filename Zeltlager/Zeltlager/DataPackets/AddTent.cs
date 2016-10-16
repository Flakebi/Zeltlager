using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zeltlager.DataPackets
{
	using Client;

	public class AddTent : DataPacket
	{
		public AddTent() { }

		public AddTent(Tent tent)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
                output.Write(tent.Number);
				output.Write(tent.Name);
				output.Write(tent.Girls);
				output.Write((ushort)tent.Supervisors.Count);
                for (int i = 0; i < tent.Supervisors.Count; i++)
                    output.Write(tent.Supervisors[i].Id);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager, Collaborator collaborator)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
                TentId tentId = new TentId();
                tentId.collaborator = collaborator;
                // Use the next free member id and inrcease the id afterwards.
                tentId.id = collaborator.NextTentId++;
                byte number = input.ReadByte();
				string name = input.ReadString();
				bool girls = input.ReadBoolean();
				ushort length = input.ReadUInt16();
				List<Member> supervisors = new List<Member>(length);
				for (int i = 0; i < length; i++)
				{
                    MemberId id = input.ReadMemberId(lager);
					supervisors.Add(lager.Members.First(m => m.Id == id));
				}

				Tent tent = new Tent(tentId, number, name, girls, supervisors);
				lager.AddTent(tent);
			}
		}
	}
}
