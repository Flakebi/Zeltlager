using System;
using System.Collections.Generic;

namespace Zeltlager
{
	/// <summary>
	/// Stores the status of a lager: How many packets were created by each collaborator.
	/// </summary>
	public class LagerStatus
	{
		/// <summary>
		/// The collaborator list saves the packet count for each collaborator.
		/// The index in this list is the collaborator id as seen from the owner of this object.
		/// </summary>
		public List<Tuple<Collaborator, ushort>> PacketCount { get; set; }

		public LagerStatus()
		{
			PacketCount = new List<Tuple<Collaborator, ushort>>();
		}
	}
}
