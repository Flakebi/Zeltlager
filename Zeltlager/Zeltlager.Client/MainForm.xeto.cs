using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace Zeltlager.Client
{
	using Network;
	using Requests = CommunicationPackets.Requests;
	using Responses = CommunicationPackets.Responses;

	public class MainForm : Form
	{
		string Status { set { statusLabel.Text = value; } }

		readonly INetworkClient client = new TcpNetworkClient();
		INetworkConnection connection;

		Label statusLabel;

		public MainForm()
		{
			XamlReader.Load(this);
			Icon = Icon.FromResource("Zeltlager.Client.icon.ico");
		}

		protected async void Connect(object sender, EventArgs e)
		{
			connection = await client.OpenConnection("localhost", LagerManager.PORT);
			Status = "Connected";
		}

		protected async void ListLagers(object sender, EventArgs e)
		{
			if (connection == null)
			{
				Status = "Not connected";
				return;
			}
			Status = "Requesting lager list";
			await connection.WritePacket(new Requests.ListLagers());
			var packet = (Responses.ListLagers)await connection.ReadPacket();
			if (packet != null)
			{
				Status = "Got lager list";
			} else
			{
				Status = "Got no packet";
			}
		}

		protected void HandleQuit(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}
	}
}
