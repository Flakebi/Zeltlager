using System;
using System.Collections.Generic;

using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;

namespace Zeltlager.Client
{
	using Network;
	using Requests = CommunicationPackets.Requests;
	using Responses = CommunicationPackets.Responses;

	public class MainForm : Form
	{
		public MainForm()
		{
			XamlReader.Load(this);
		}

		protected void Connect(object sender, EventArgs e)
		{
			MessageBox.Show("I was clicked!");
		}

		protected void ListLagers(object sender, EventArgs e)
		{
			MessageBox.Show("I was clicked!");
		}

		protected void HandleQuit(object sender, EventArgs e)
		{
			Application.Instance.Quit();
		}
	}
}
