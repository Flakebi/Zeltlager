using System;
using System.IO;
using System.Threading.Tasks;

using Eto;
using Eto.Forms;

namespace Zeltlager.Client.Desktop
{
	public class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Task.WaitAll(AsyncMain(args));
		}

		static async Task AsyncMain(string[] args)
		{
			var app = new Application(Platform.Detect);
			var io = new RootedIoProvider(new ServerIoProvider(), Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
			var main = new MainForm(io);
			await main.Load();
			app.Run(main);
		}
	}
}
