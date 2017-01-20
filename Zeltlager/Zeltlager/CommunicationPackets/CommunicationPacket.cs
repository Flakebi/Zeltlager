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
		const int VERSION = 0;

		static readonly Type[] packetTypes = {
			typeof(Requests.Bundles),
			typeof(Requests.CollaboratorData),
			typeof(Requests.LagerStatus),
			typeof(Requests.ListLagers),
			typeof(Requests.Register),
			typeof(Requests.UploadBundle),
			typeof(Responses.Bundle),
			typeof(Responses.CollaboratorData),
			typeof(Responses.LagerStatus),
			typeof(Responses.ListLagers),
			typeof(Responses.Register),
			typeof(Responses.Status)
		};

		/// <summary>
		/// Read a packet. This function can throw an IOException, e.g. if the packet type is invalid.
		/// </summary>
		/// <param name="data">The input data.</param>
		/// <returns>The read packet.</returns>
		public static CommunicationPacket ReadPacket(byte[] data)
		{
			using (BinaryReader input = new BinaryReader(new MemoryStream(data)))
			{
				int packetType = input.ReadInt32();

				if (packetType >= packetTypes.Length)
					throw new LagerException("Invalid communication packet type");

				// Create a new packet of the specified type using the default constructor
				CommunicationPacket packet = (CommunicationPacket)packetTypes[packetType].GetTypeInfo().DeclaredConstructors
					.First(ctor => ctor.GetParameters().Length == 0).Invoke(new object[0]);

				// Fill the packet data
				packet.Data = input.ReadBytes((int)(input.BaseStream.Length - input.BaseStream.Position));
				return packet;
			}
		}

		protected byte[] Data { get; set; }

		public void WritePacket(BinaryWriter output)
		{
			var index = Array.IndexOf(packetTypes, GetType());
			if (index == -1)
				throw new InvalidOperationException("Trying to write an unknown packet type, you should add this packet to the CommunicationPacket.packetTypes array.");
			output.Write(index);
			output.Write(Data);
		}
	}
}
