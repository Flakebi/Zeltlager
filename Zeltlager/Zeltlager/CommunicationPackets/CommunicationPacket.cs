using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zeltlager.CommunicationPackets
{
    /// <summary>
    /// A subclass of this type must have a default constructor.
    /// All possible packet types have to be added to the packetTypes array.
    /// </summary>
    public abstract class CommunicationPacket
    {
        /// <summary>
        /// The version of the data packet protocol.
        /// </summary>
        static uint VERSION = 0;

        static Type[] packetTypes = {
            typeof(ListGames),
        };

        /// <summary>
        /// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
        /// </summary>
        /// <param name="input">The input reader</param>
        /// <returns>The read packet.</returns>
        public static CommunicationPacket ReadPacket(BinaryReader input)
        {
            byte packetType = input.ReadByte();

            if (packetType >= packetTypes.Length)
                throw new IOException("Invalid packet type");

            // Create a new packet of the specified type using the default constructor
            CommunicationPacket packet = (CommunicationPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
                .First(ctor => ctor.GetParameters().Length == 0).Invoke(new object[0]);

            // Fill the packet data
            packet.Data = new byte[input.BaseStream.Length - input.BaseStream.Position];
            input.Read(packet.Data, 0, packet.Data.Length);
            return packet;
        }

        protected byte[] Data { get; set; }

        public CommunicationPacket()
        {
        }

        public void WritePacket(BinaryWriter output)
        {
            output.Write(Data);
        }
    }
}
