using System.Linq;

namespace Zeltlager.DataPackets
{
	public class DeleteSupervisorFromTent : DataPacket
	{
		public DeleteSupervisorFromTent() { }

		public DeleteSupervisorFromTent(Member supervisor, Tent tent)
		{
			Data = new byte[3];
			supervisor.Id.ToBytes(Data, 0);
			Data[2] = tent.Number;
		}

		public override void Deserialise(Lager lager)
		{
			ushort id = Data.ToUShort(0);
			byte number = Data[2];
			Member supervisor = lager.Members.First(m => m.Id == id);
			Tent tent = lager.Tents.First(t => t.Number == number);
			tent.RemoveSupervisor(supervisor);
		}
	}
}
