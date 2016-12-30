using System;
using System.Linq;
using System.Threading.Tasks;

using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace Zeltlager.Client
{
	using DataPackets;
	using Network;
	using Serialisation;
	using Requests = CommunicationPackets.Requests;
	using Responses = CommunicationPackets.Responses;

	public class MainForm : Form
	{
		LagerClientManager manager;
		LagerClient lager;
		
		string Status { set { statusLabel.Text = value; } }

		readonly INetworkClient client = new TcpNetworkClient();
		INetworkConnection connection;

		// Disable the not assigned warning, the field will be assigned from the xaml.
#pragma warning disable 0649
		Label statusLabel;
#pragma warning restore 0649

		public MainForm(IIoProvider io)
		{
			XamlReader.Load(this);
			Icon = Icon.FromResource("Zeltlager.Client.icon.ico");

			LagerManager.IsClient = true;
			manager = new LagerClientManager(io);
			manager.NetworkClient = new TcpNetworkClient();
		}

		public async void LoadLagers(object sender, EventArgs e)
		{
			// Load LagerManager
			Status = "Load settings";
			await LagerManager.Log.Load();
			await manager.Load();
			Status = manager.Lagers.Count + " lagers loaded";

			// Load lager
			Status = "Load lager";
			int lagerId = manager.Settings.LastLager;
			lager = (LagerClient)manager.Lagers[lagerId];
			if (!await lager.LoadBundles())
				Status = "Error while loading the lager files";
			if (!await lager.ApplyHistory())
				Status = "Error while loading the lager";
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

		protected async void CreateLager(object sender, EventArgs e)
		{
			var lager = await manager.CreateLager("test", "pass", status => Status = status.ToString());
			await lager.CreateTestData();
		}

		protected async void AddMember(object sender, EventArgs e)
		{
			if (!manager.Lagers.ContainsKey(0))
			{
				Status = "No lager 0 loaded";
				return;
			}
			var lager = (LagerClient)manager.Lagers[0];
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(manager, lager);
			context.PacketId = new PacketId(lager.OwnCollaborator);
			await lager.AddPacket(await AddPacket.Create(lager.ClientSerialiser, context,
				new Member(null, "Anna", lager.Tents.Skip(new Random().Next(0, lager.Tents.Count)).First(), true, lager)));
			Status = "Member added";
		}

		protected void HandleQuit(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}
	}
}
