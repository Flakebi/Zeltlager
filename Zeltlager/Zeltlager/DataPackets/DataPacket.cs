using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	public abstract class DataPacket
	{
		static Type[] packetTypes = {
			typeof(GeneralDataPacket)
		};

		static TypeInfo GetPacketType(TypeInfo type)
		{
			return type.DeclaredNestedTypes
				.Where(t => t.Name == "PacketType" && t.IsEnum).First();
		}

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <param name="input">The input reader</param>
		/// <param name="lager">The object where the read packet should be applied.</param>
		/// <returns>The read packet.</returns>
		public static DataPacket ReadDataPacket(BinaryReader input, Lager lager)
		{
			byte packetType = input.ReadByte();
			foreach (var type in packetTypes)
			{
				var typeInfo = type.GetTypeInfo();
				var packetTypeInfo = GetPacketType(typeInfo);

				byte types = (byte)(Enum.GetValues(packetTypeInfo.GetType())
					.Cast<int>().Max() + 1);
				if (packetType < types)
					return (DataPacket)typeInfo.DeclaredConstructors
					   .Where(ctor =>
					   {
						   var parameters = ctor.GetParameters();
						   return parameters.Length == 3 &&
							parameters[0].GetType() == typeof(BinaryReader) &&
							parameters[1].GetType() == packetTypeInfo.GetType() &&
							parameters[2].GetType() == typeof(Lager);
					   }).First().Invoke(new object[] { input, packetType, lager });
				packetType -= types;
			}
			throw new IOException("Invalid packet type");
		}
	}
}
