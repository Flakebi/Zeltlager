using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Eto.Drawing;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace Zeltlager.Client
{
	using DataPackets;
	using Network;
	using Serialisation;

	class WrapTextBinding<T> : IIndirectBinding<string>
	{
		Func<T, string> wrapper;

		public WrapTextBinding(Func<T, string> wrapper)
		{
			this.wrapper = wrapper;
		}

		public string GetValue(object dataItem)
		{
			return wrapper((T)dataItem);
		}

		public void SetValue(object dataItem, string value)
		{
			throw new InvalidOperationException("Only a getter");
		}

		public void Unbind()
		{
			throw new InvalidOperationException("Only a getter");
		}

		public void Update(BindingUpdateMode mode = BindingUpdateMode.Source)
		{
			throw new InvalidOperationException("Only a getter");
		}
	}

	public class MainForm : Form
	{
		LagerClientManager manager;
		LagerClient lager;
		Dictionary<int, LagerData> serverLagers;

		string serverPassword;
		
		string Status
		{
			get { return statusLabel.Text; }			
			set { statusLabel.Text = value; statusTimer.Start(); }
		}

		IIoProvider globalIoProvider;

		// Disable the not assigned warning, the fields will be assigned from the xaml.
#pragma warning disable 0649
		StackLayout topBarLayout;
		Label statusLabel;
		DropDown lagerDropDown;
		DropDown collaboratorDropDown;

		Panel downloadContent;
		TextBox downloadPasswordText;
		Label lagerInfoLabel;
		Button lagerDownloadButton;
#pragma warning restore 0649

		/// <summary>
		/// Remove the status message after some time.
		/// </summary>
		UITimer statusTimer = new UITimer();

		List<Control> contents = new List<Control>();

		public MainForm(IIoProvider io, IIoProvider globalIo)
		{
			XamlReader.Load(this);
			Icon = Icon.FromResource("Zeltlager.Client.icon.ico");
			var size = MinimumSize;
			size.Width = topBarLayout.Width;
			if (size.Width <= 0)
				size.Width = 100;
			MinimumSize = size;

			LagerManager.IsClient = true;
			manager = new LagerClientManager(io);
			manager.NetworkClient = new TcpNetworkClient();
			if (manager.Settings.ServerAddress == null)
				manager.Settings.ServerAddress = "flakebi.de";

			globalIoProvider = globalIo;

			contents.Add(downloadContent);
			statusTimer.Interval = 5;
			statusTimer.Elapsed += (sender, e) =>
			{
				Status = "";
				statusTimer.Stop();
			};

			lagerDropDown.ItemTextBinding = new WrapTextBinding<Tuple<int, LagerClient>>(t => t.Item2.Data.Name);
			lagerDropDown.ItemKeyBinding = new WrapTextBinding<Tuple<int, LagerClient>>(t => t.Item1.ToString());
			collaboratorDropDown.ItemTextBinding = new WrapTextBinding<Collaborator>(c => c.ToString());
			collaboratorDropDown.ItemKeyBinding = new WrapTextBinding<Collaborator>(c => c.Key.Modulus.ToHexString());

			ShowContent(null);
		}

		public async Task LoadLagers()
		{
			try
			{
				Status = "Load log";
				await LagerManager.Log.Load();
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("Load log", e);
			}
			// Load LagerManager
			try
			{
				Status = "Load lagers";
				await manager.Load();
			} catch (Exception e)
			{
				await LagerManager.Log.Exception("Load lagers", e);
			}
			Status = manager.Lagers.Count + " lagers loaded";
			// Add the names to the dropdown
			lagerDropDown.DataStore = manager.Lagers.Select(pair => new Tuple<int, LagerClient>(pair.Key, (LagerClient)pair.Value));

			// Load the last lager
			Status = "Load lager";
			int lagerId = manager.Settings.LastLager;
			if (!manager.Lagers.ContainsKey(lagerId))
			{
				Status = "No last lager found";
				return;
			}
			lager = (LagerClient)manager.Lagers[lagerId];
			// Pick the current lager in the drop down menu
			lagerDropDown.SelectedKey = lagerId.ToString();
		}

		void ShowContent(Control content)
		{
			foreach (var c in contents)
				c.Visible = false;
			if (content != null)
				content.Visible = true;
		}

		async void ListLagers(object sender, EventArgs args)
		{
			try
			{
				serverLagers = await manager.RemoteListLagers(status => Status = "Network: " + status);
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("List lagers", e);
				Status = "Error: " + e;
			}

			Status = "Got " + serverLagers.Count + " lagers";

			// Show the download content panel
			lagerInfoLabel.Text = "";
			lagerDownloadButton.Visible = false;
			ShowContent(downloadContent);
		}

		async void Decrypt(object sender, EventArgs args)
		{
			lagerInfoLabel.Text = string.Empty;
			lagerDownloadButton.Visible = false;
			if (serverLagers == null)
			{
				Status = "No lagers available";
				return;
			}
			serverPassword = downloadPasswordText.Text;
			foreach (var d in serverLagers)
			{
				if (await d.Value.Decrypt(serverPassword))
				{
					Status = "Success for lager " + d.Key;
					// Display the lager info
					lagerInfoLabel.Text = "Name: " + d.Value.Name + "\nSymmetricKey: " + d.Value.SymmetricKey.ToHexString();
					lagerDownloadButton.Visible = true;
					return;
				}
			}
			Status = "No lager found";
		}

		async void DownloadLager(object sender, EventArgs args)
		{
			foreach (var d in serverLagers)
			{
				if (await d.Value.Decrypt(serverPassword))
				{
					try
					{
						lager = await manager.DownloadLager(d.Key, d.Value, serverPassword, status => Status = "Initing lager: " + status, status => Status = "Downloading lager: " + status);
					}
					catch (Exception e)
					{
						await LagerManager.Log.Exception("Download lager", e);
						Status = "Error: " + e;
					}
					return;
				}
			}
		}

		async void CreateLager(object sender, EventArgs args)
		{
			lager = await manager.CreateLager("test", "pass", status => Status = "Create lager: " + status);
		}

		async void Synchronise(object sender, EventArgs args)
		{
			try
			{
				await lager.Synchronise(status => Status = "Synchronise lager: " + status);
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("Synchronise lager", e);
				Status = "Error: " + e;
			}
		}

		async void SelectedLagerChanged(object sender, EventArgs args)
		{
			// Unload the previous lager
			lager?.Unload();
			// Get the currently selected lager
			lager = (LagerClient)manager.Lagers[int.Parse(lagerDropDown.SelectedKey)];
			// Load the newly selected lager
			bool success = true;
			if (!await lager.LoadBundles())
			{
				Status = "Error while loading the lager files";
				success = false;
			}
			lager.Reset();
			if (!await lager.ApplyHistory())
			{
				Status = "Error while loading the lager";
				success = false;
			}
			if (success)
				Status = "Lager loaded";

			collaboratorDropDown.DataStore = lager.Collaborators.Values;
		}

		async void Upload(object sender, EventArgs args)
		{
			if (lager == null)
			{
				Status = "No lager loaded";
				return;
			}
			try
			{
				await lager.Synchronise(status => Status = "Upload lager: " + status);
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("Upload lager", e);
				Status = "Error: " + e;
			}
		}

		async void AddMember(object sender, EventArgs args)
		{
			if (lager == null)
			{
				Status = "No lager loaded";
				return;
			}
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
			context.PacketId = new PacketId(lager.OwnCollaborator);
			await lager.AddPacket(await AddPacket.Create(lager.ClientSerialiser, context,
				new Member(null, "Anna", lager.Tents.Skip(new Random().Next(0, lager.Tents.Count)).First(), true, lager)));
			Status = "Member added";
		}

		void DownloadPasswordTextKeyUp(object sender, KeyEventArgs args)
		{
			if (args.Key == Keys.Enter)
				Decrypt(null, null);
		}

		void Quit(object sender, EventArgs args)
		{
			Application.Instance.Quit();
		}

		async void CreateLagerFromFile(object sender, EventArgs args)
		{
			if (lager == null)
			{
				Status = "There is no Lager at the moment!";
				return;
			}
			OpenFileDialog dialog = new OpenFileDialog()
			{
				Title = "Choose File with Names",
				MultiSelect = false,
			};
			DialogResult dr = dialog.ShowDialog(Parent);
			if (dr == DialogResult.Yes || dr == DialogResult.Ok)
			{
				Status = "File selected";
				string path = dialog.FileName;
				if (await globalIoProvider.ExistsFile(path))
				{
					Status = "File exists";
					Stream s = await globalIoProvider.ReadFile(path);
					using (StreamReader sr = new StreamReader(s))
					{
						await lager.AddPacket(await AddPacket.Create(lager.ClientSerialiser, new LagerClientSerialisationContext(lager), new Tent(null, 0, "Standardzelt", true, null, lager)));
						while (!sr.EndOfStream)
						{
							string name = sr.ReadLine();
							Status = "Add " + name;
							await lager.AddPacket(await AddPacket.Create(lager.ClientSerialiser,
								new LagerClientSerialisationContext(lager), new Member(null, name, lager.Tents.First(), false, lager)));
						}
					}
				}
			}
			Status = "Creating from File finished :)";
		}
	}
}
