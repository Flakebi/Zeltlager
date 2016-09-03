using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zeltlager.DataPackets;

namespace Zeltlager.Server
{
	class Server
	{
		static void Main(string[] args)
		{
			//Task.Factory.StartNew(async () =>
			Lager lager = new Lager(0, "default", "pass");
			Lager.IoProvider = new RootedIoProvider(new ServerIoProvider(), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Lager"));
			try
			{
				lager.Init();
				Tent tent = new Tent(0, "Regenbogenforellen", new List<Member>());
				DataPacket packet = new AddTentPacket(tent);
				lager.Collaborators.First().AddPacket(packet);
				packet = new AddMemberPacket(new Member(0, "Caro", tent, true));
				lager.Collaborators.First().AddPacket(packet);
				//lager.Load(Lager.IoProvider).Wait();
				lager.Save(Lager.IoProvider).Wait();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
