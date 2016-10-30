using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	public class ClientSettings
	{
		const string SETTINGS_FILE = "settings.conf";
		const int VERSION = 0;

		public int LastLager { get; set; }

		//TODO Replace by serialising
		public async Task Load()
		{
			// Set default values
			LastLager = 0;
			Lagers.Clear();

			IIoProvider io = LagerBase.IoProvider;
			if (await io.ExistsFile(SETTINGS_FILE))
			{
				using (BinaryReader input = new BinaryReader(await io.ReadFile(SETTINGS_FILE)))
				{
					if (input.ReadInt32() == VERSION)
					{
						//TODO Don't read lager count
						int lagerCount = input.ReadInt32();
						int lastLager = input.ReadInt32();
						if (lastLager < lagerCount)
						{
							LastLager = lastLager;
							// Read list
							Lagers.Capacity = lagerCount;
							for (int i = 0; i < lagerCount; i++)
							{
								string name = input.ReadString();
								string pass = input.ReadString();
								Lagers.Add(new Tuple<string, string>(name, pass));
							}
						}
					}
				}
			}
		}

		public async Task Save()
		{
			IIoProvider io = LagerBase.IoProvider;
			using (BinaryWriter output = new BinaryWriter(await io.WriteFile(SETTINGS_FILE)))
			{
				output.Write(VERSION);
				output.Write(Lagers.Count);
				output.Write(LastLager);

				// Write list
				for (int i = 0; i < Lagers.Count; i++)
				{
					output.Write(Lagers[i].Item1);
					output.Write(Lagers[i].Item2);
				}
			}
		}
	}
}
