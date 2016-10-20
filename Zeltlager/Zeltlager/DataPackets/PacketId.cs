namespace Zeltlager.DataPackets
{
    public class PacketId
    {
        public readonly Collaborator Creator;
        public readonly DataPacketBundle Bundle;
        public readonly byte? PacketIndex;

        public PacketId(Collaborator creator, DataPacketBundle bundle = null, byte? packetIndex = null)
        {
            Creator = creator;
            Bundle = bundle;
            PacketIndex = packetIndex;
        }

        public PacketId Clone(Collaborator creator)
        {
            return new PacketId(creator, null, null);
        }

        public PacketId Clone(DataPacketBundle bundle)
        {
            return new PacketId(Creator, bundle, null);
        }

        public PacketId Clone(byte packetIndex)
        {
            return new PacketId(Creator, Bundle, packetIndex);
        }
    }
}
