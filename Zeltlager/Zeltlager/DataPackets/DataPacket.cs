using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	/// <summary>
	/// A subclass of this type must have a constructor that takes
	/// (BinaryReader input, Lager lager)
	/// A template for a packet can be found at the end of this file.
	/// </summary>
	public abstract class DataPacket
	{
		/// <summary>
		/// The version of the packet protocol.
		/// </summary>
		static uint VERSION = 0;

		static Type[] packetTypes = {
			typeof(AddMemberPacket),
			typeof(DeleteMemberPacket),
		};

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <param name="input">The input reader</param>
		/// <param name="lager">The object where the read packet should be applied.</param>
		/// <returns>The read packet.</returns>
		public static DataPacket ReadDataPacket(BinaryReader input, Lager lager)
		{
			DateTime timestamp = DateTime.FromBinary(input.ReadInt64());
			byte packetType = input.ReadByte();

			if (packetType >= packetTypes.Length)
				throw new IOException("Invalid packet type");
			DataPacket packet = (DataPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
				.First(ctor =>
				{
					var parameters = ctor.GetParameters();
					return parameters.Length == 2 &&
						parameters[0].ParameterType == typeof(BinaryReader) &&
						parameters[2].ParameterType == typeof(Lager);
				}).Invoke(new object[] { input, packetType, lager });

			packet.timestamp = timestamp;
			return packet;
		}

		protected DateTime timestamp;

		public void WritePacket(BinaryWriter output)
		{
			output.Write(timestamp.ToBinary());
			// Write the type of this packet
			output.Write((byte)Array.IndexOf(packetTypes, GetType()));

			WritePacketData(output);
		}

		protected abstract void WritePacketData(BinaryWriter output);
		public abstract void Apply(Lager lager);
	}
}

/*******************************************************************************

class XXXDataPacket : DataPacket
{
	public XXXDataPacket(BinaryReader input, Lager lager)
	{
	}

	public GeneralDataPacket()
	{
	}

	protected override void WritePacketData(BinaryWriter output)
	{
	}

	public override void Apply(Lager lager)
	{
	}
}

*******************************************************************************/
