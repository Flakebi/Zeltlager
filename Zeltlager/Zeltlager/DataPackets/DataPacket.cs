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
			typeof(AddTentPacket),
			typeof(DeleteTentPacket),
			typeof(AddSupervisorToTentPacket),
			typeof(DeleteSupervisorFromTentPacket),
		};

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <param name="input">The input reader</param>
		/// <param name="lager">The object where the read packet should be applied.</param>
		/// <returns>The read packet.</returns>
		public static DataPacket ReadDataPacket(BinaryReader input, Lager lager)
		{
			byte packetType = input.ReadByte();

			if (packetType >= packetTypes.Length)
			{
				// Create a new InvalidDataPacket
				byte[] data = new byte[1 + input.BaseStream.Length];
				data[0] = packetType;
				input.Read(data, 1, data.Length - 1);
				return new InvalidDataPacket(data);
			}

			DateTime timestamp = DateTime.FromBinary(input.ReadInt64());

			DataPacket packet = (DataPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
				.First(ctor =>
				{
					var parameters = ctor.GetParameters();
					return parameters.Length == 2 &&
						parameters[0].ParameterType == typeof(BinaryReader) &&
						parameters[2].ParameterType == typeof(Lager);
				}).Invoke(new object[] { input, packetType, lager });

			packet.Timestamp = timestamp;
			return packet;
		}

		/// <summary>
		/// Only set when available.
		/// </summary>
		public byte[] Signature { get; set; }
		public byte[] Iv { get; set; }

		public DateTime Timestamp { get; protected set; }

		public DataPacket()
		{
			Timestamp = new DateTime();
		}

		public void WritePacket(BinaryWriter output)
		{
			// Don't write the header for InvalidDataPackets.
			if (!(this is InvalidDataPacket))
			{
				// Write the type of this packet
				output.Write((byte)Array.IndexOf(packetTypes, GetType()));
				output.Write(Timestamp.ToBinary());
			}

			WritePacketData(output);
		}

		protected abstract void WritePacketData(BinaryWriter output);

		/// <summary>
		/// Applies the content of this packet to a Lager.
		/// </summary>
		/// <param name="lager">The Lager to which this packet should be applied.</param>
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
