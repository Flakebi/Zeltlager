using System.Linq;

namespace Zeltlager.DataPackets
{
	public class DeleteSupervisorFromTentPacket : DataPacket
	{
		ushort id;
		byte number;

		public DeleteSupervisorFromTentPacket() { }

		public DeleteSupervisorFromTentPacket(Member supervisor, Tent tent)
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
			tent.RemoveSupervisor(supervisor);
		}
	}
}
