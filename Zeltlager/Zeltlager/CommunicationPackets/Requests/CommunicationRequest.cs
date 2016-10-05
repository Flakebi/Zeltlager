﻿using System;
using System.IO;

using Zeltlager.Network;

namespace Zeltlager.CommunicationPackets.Requests
{
	public abstract class CommunicationRequest : CommunicationPacket
	{
		/// <summary>
		/// Apply the request to a lager and send a response.
		/// </summary>
		/// <param name="lagerProvider"></param>
		public abstract void Apply(INetworkConnection connection, ILagerProvider lagerProvider);
	}
}
