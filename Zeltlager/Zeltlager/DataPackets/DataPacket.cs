using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
    abstract class DataPacket
    {
        enum DataPacketType : byte
        {
            General,
            Tournament,
            Competition,
            Erwischt,
            Calendar,
            // Special types
            Bundle,
        }

        static Type[] packetTypes = {
            typeof(GeneralDataPacket)
        };

        /// <summary>
        /// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
        /// </summary>
        /// <param name="input">The input reader</param>
        /// <param name="zeltlager">The object where the read packet should be applied.</param>
        /// <returns>The read packet.</returns>
        public static DataPacket ReadDataPacket(BinaryReader input, Lager zeltlager)
        {
            byte type = input.ReadByte();
            if (type >= packetTypes.Length)
                throw new IOException("Invalid packet type");
            return (DataPacket) packetTypes[type].GetTypeInfo().DeclaredConstructors.Where(ctor =>
            {
                var parameters = ctor.GetParameters();
                return parameters.Length == 2 &&
                    parameters[0].GetType() == typeof(BinaryReader) &&
                    parameters[1].GetType() == typeof(Lager);
            }).First().Invoke(new object[] { input, zeltlager });
        }

        DataPacketType Type { get; set; }
    }
}
