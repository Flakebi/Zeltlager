using System.Linq;

namespace Zeltlager.DataPackets
{
	public class DeleteTentPacket : DataPacket
	{
		byte number;

		public DeleteTentPacket() { }

		public DeleteTentPacket(Tent tent)
		{
			number = tent.Number;
		}

		public override void Serialise()
		{
			Data = new byte[] { number };
		}

		public override void Deserialise(Lager lager)
		{
			number = Data[0];
			Tent tent = lager.Tents.First(t => t.Number == number);
			lager.RemoveTent(tent);
		}
	}
}
