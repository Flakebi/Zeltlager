using System;

using Xwt;
using Xwt.Drawing;

namespace Zeltlager.Client
{
	using Network;
	using Requests = CommunicationPackets.Requests;
	using Responses = CommunicationPackets.Responses;

	class Client : Window
	{
		readonly INetworkClient client = new TcpNetworkClient();
		INetworkConnection connection;
		
		[STAThread]
		static void Main(string[] args)
		{
			Application.Initialize(ToolkitType.Gtk);
			var mainWindow = new Client();
			mainWindow.Show();
			Application.Run();
			mainWindow.Dispose();
			Application.Dispose();
		}

		public Client()
		{
			Title = "Zeltlager";
			Width = 500;
			Height = 400;
			Icon = Image.FromResource(GetType(), "icon.ico");

			VBox content = new VBox();

			Label statusLabel = new Label();
			content.PackStart(statusLabel, false, false);

			Button connectButton = new Button("Connect");
			connectButton.Clicked += async delegate
			{
				connection = await client.OpenConnection("localhost", LagerManager.PORT);
				statusLabel.Text = "Connected";
			};
			content.PackStart(connectButton, false, false);
			Button listButton = new Button("List lagers");
			listButton.Clicked += async delegate
			{
				if (connection == null)
				{
					statusLabel.Text = "Not connected";
					return;
				}
				statusLabel.Text = "Requesting lager list";
				await connection.WritePacket(new Requests.ListLagers());
				var packet = (Responses.ListLagers)await connection.ReadPacket();
				if (packet != null)
				{
					statusLabel.Text = "Got lager list";
				}
				else
				{
					statusLabel.Text = "Got no packet";
				}
			};
			content.PackStart(listButton, false, false);
			Content = content;

			Closed += (sender, eventArgs) => Application.Exit();
		}
	}
}
