using System.Linq;

namespace Zeltlager.DataPackets
{
	public class DeleteTentPacket : DataPacket
	{
		public DeleteTentPacket() { }

		public DeleteTentPacket(Tent tent)
		{
			Data = new byte[] { tent.Number };
		}

		public override void Deserialise(Lager lager)
		{
			byte number = Data[0];
			Tent tent = lager.Tents.First(t => t.Number == number);
			lager.RemoveTent(tent);
		}
	}
}
