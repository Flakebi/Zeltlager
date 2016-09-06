using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zeltlager.DataPackets
{
	public class AddTentPacket : DataPacket
	{
		Tent tent;

		public AddTentPacket() { }

		public AddTentPacket(Tent tent)
		{
			this.tent = tent;
		}

		public override void Serialise()
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(tent.Number);
				output.Write(tent.Name);
				output.Write(tent.Supervisors.Count);
				for (int i = 0; i < tent.Supervisors.Count; i++)
					output.Write(tent.Supervisors[i].Id);
				Data = mem.ToArray();
			}
		}

		public override void Deserialise(Lager lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				byte number = input.ReadByte();
				string name = input.ReadString();
				ushort length = input.ReadUInt16();
				List<Member> supervisors = new List<Member>(length);
				for (int i = 0; i < length; i++)
				{
					ushort id = input.ReadUInt16();
					supervisors.Add(lager.Members.First(m => m.Id == id));
				}

				Tent tent = new Tent(number, name, supervisors);
				lager.AddTent(tent);
			}
		}
	}
}
