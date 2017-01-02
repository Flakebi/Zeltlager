using System;
using System.IO;

using Eto;
using Eto.Forms;

namespace Zeltlager.Client.Desktop
{
	public class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var app = new Application(Platform.Detect);
			var io = new RootedIoProvider(new DesktopIoProvider(), Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
			var main = new MainForm(io);
			app.AsyncInvoke(async () => await main.LoadLagers());
			app.AsyncInvoke(() => LagerManager.Log.OnMessage += Console.WriteLine);
			app.Run(main);
		}
	}
}
