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
				tent.Id.Write(output);
				output.Write(tent.Name);
				output.Write((ushort)tent.Supervisors.Count);
				for (int i = 0; i < tent.Supervisors.Count; i++)
					tent.Supervisors[i].Id.Write(output);
			}
			Data = mem.ToArray();
		}

		public override void Deserialise(LagerClient lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				TentId tentId = new TentId(lager, input);
				string name = input.ReadString();
				ushort length = input.ReadUInt16();
				List<Member> supervisors = new List<Member>(length);
				for (int i = 0; i < length; i++)
				{
					MemberId id = new MemberId(lager, input);
					supervisors.Add(lager.Members.First(m => m.Id == id));
				}

				Tent tent = new Tent(tentId, name, supervisors);
				lager.AddTent(tent);
			}
		}
	}
}
