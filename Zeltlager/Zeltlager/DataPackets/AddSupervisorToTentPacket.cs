using System.Linq;

namespace Zeltlager.DataPackets
{
	public class AddSupervisorToTentPacket : DataPacket
	{
		ushort id;
		byte number;

		public AddSupervisorToTentPacket() { }

		public AddSupervisorToTentPacket(Member supervisor, Tent tent)
		{
			id = supervisor.Id;
			number = tent.Number;
		}

		public override void Serialise()
		{
			Data = new byte[3];
			id.ToBytes(Data, 0);
			Data[2] = number;
		}

		public override void Deserialise(Lager lager)
		{
			id = Data.ToUShort(0);
			number = Data[2];
			Member supervisor = lager.Members.First(m => m.Id == id);
			Tent tent = lager.Tents.First(t => t.Number == number);
			tent.AddSupervisor(supervisor);
		}
	}
}
