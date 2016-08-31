using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zeltlager.DataPackets;

namespace Zeltlager
{
	public class Collaborator
	{
		List<DataPacket> packets = new List<DataPacket>();

		public byte Id { get; set; }
		public List<DataPacket> Packets { get { return packets; } }

		public async Task<bool> Save(IIoProvider io)
		{
			string id = Id.ToString();
			if (!await io.ExistsFolder(id))
				await io.CreateFolder(id);

			for (int i = 0; i < packets.Count; i++)
			{
				var output = await io.WriteFile(Path.Combine(id, i.ToString()));
				packets[i].WritePacket(output);
			}

			return true;
		}
	}
}
