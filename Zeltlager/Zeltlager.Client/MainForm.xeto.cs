using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace Zeltlager.Client
{
	using DataPackets;
	using Network;
	using Requests = CommunicationPackets.Requests;
	using Responses = CommunicationPackets.Responses;
	using Serialisation;

	public class MainForm : Form
	{
		readonly INetworkClient client = new TcpNetworkClient();
		INetworkConnection connection;

		LagerClientManager manager;
		LagerClient lager;
		Dictionary<int, LagerData> serverLagers;
		
		string Status
		{
			get { return statusLabel.Text; }			
			set { statusLabel.Text = value; statusTimer.Start(); }
		}

		// Disable the not assigned warning, the fields will be assigned from the xaml.
#pragma warning disable 0649
		Label statusLabel;
		Button downloadButton;
		Panel downloadContent;
		TextBox downloadPasswordText;
#pragma warning restore 0649
		/// <summary>
		/// Remove the status message after some time.
		/// </summary>
		UITimer statusTimer = new UITimer();

		List<Control> contents = new List<Control>();

		public MainForm(IIoProvider io)
		{
			XamlReader.Load(this);
			Icon = Icon.FromResource("Zeltlager.Client.icon.ico");

			LagerManager.IsClient = true;
			manager = new LagerClientManager(io);
			manager.NetworkClient = new TcpNetworkClient();

			contents.Add(downloadContent);
			statusTimer.Interval = 5;
			statusTimer.Elapsed += (sender, e) =>
			{
				Status = "";
				statusTimer.Stop();
			};

			ShowContent(null);
		}

		public async Task LoadLagers()
		{
			// Load LagerManager
			Status = "Load settings";
			await LagerManager.Log.Load();
			await manager.Load();
			Status = manager.Lagers.Count + " lagers loaded";

			// Load lager
			Status = "Load lager";
			int lagerId = manager.Settings.LastLager;
			if (!manager.Lagers.ContainsKey(lagerId))
			{
				Status = "No lager found";
				return;
			}
			lager = (LagerClient)manager.Lagers[lagerId];
			if (!await lager.LoadBundles())
			{
				Status = "Error while loading the lager files";
				return;
			}
			if (!await lager.ApplyHistory())
			{
				Status = "Error while loading the lager";
				return;
			}
			Status = "Lager loaded";
		}

		void ShowContent(Control content)
		{
			foreach (var c in contents)
				c.Visible = false;
			if (content != null)
				content.Visible = true;
		}

		async void Connect(object sender, EventArgs args)
		{
			bool success = false;
			try
			{
				connection = await client.OpenConnection("localhost", LagerManager.PORT);
				success = true;
			}
			catch (Exception e)
			{
				Status = "Can't connect: " + e;
			}
			if (success)
			{
				Status = "Connected";
				downloadButton.Enabled = true;
			}
		}

		async void Download(object sender, EventArgs args)
		{
			if (connection == null)
			{
				Status = "Not connected";
				return;
			}

			Status = "Requesting lager list";
			await connection.WritePacket(new Requests.ListLagers());
			var packet = (Responses.ListLagers)await connection.ReadPacket();
			serverLagers = packet.GetLagerData();
			if (packet != null)
				Status = "Got " + serverLagers.Count + " lagers";
			else
				Status = "Got no packet";

			// Show the download content panel
			ShowContent(downloadContent);
		}

		async void Decrypt(object sender, EventArgs args)
		{
			if (serverLagers == null)
			{
				Status = "No lagers available";
				return;
			}
			string password = downloadPasswordText.Text;
			foreach (var d in serverLagers)
			{
				if (await d.Value.Decrypt(password))
				{
					Status = "Success for lager " + d.Key;
					lager = new LagerClient(manager, manager.IoProvider, d.Key);
					//TODO lager.Data = d.Value;
					return;
				}
			}
			Status = "No lager found";
		}

		async void CreateLager(object sender, EventArgs args)
		{
			lager = await manager.CreateLager("test", "pass", status => Status = status.ToString());
			await lager.CreateTestData();
		}

		async void AddMember(object sender, EventArgs args)
		{
			if (!manager.Lagers.ContainsKey(0))
			{
				Status = "No lager 0 loaded";
				return;
			}
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(manager, lager);
			context.PacketId = new PacketId(lager.OwnCollaborator);
			await lager.AddPacket(await AddPacket.Create(lager.ClientSerialiser, context,
				new Member(null, "Anna", lager.Tents.Skip(new Random().Next(0, lager.Tents.Count)).First(), true, lager)));
			Status = "Member added";
		}

		void Quit(object sender, EventArgs args)
		{
			Application.Instance.Quit();
		}
	}
}
