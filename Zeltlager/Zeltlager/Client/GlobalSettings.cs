﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	public class GlobalSettings
	{
		const string SETTINGS_FILE = "settings.conf";
		const byte VERSION = 0;

		public byte LastLager { get; set; }
		/// <summary>
		/// Contains all lagers.
		/// For each lager the name and the password.
		/// </summary>
		public List<Tuple<string, string>> Lagers { get; set; }

		public GlobalSettings()
		{
			Lagers = new List<Tuple<string, string>>();
		}

		public async Task Load()
		{
			// Set default values
			LastLager = 0;
			Lagers.Clear();

			IIoProvider io = Lager.IoProvider;
			if (await io.ExistsFile(SETTINGS_FILE))
			{
				BinaryReader input = await io.ReadFile(SETTINGS_FILE);
				if (input.ReadByte() == VERSION)
				{
					byte lagerCount = input.ReadByte();
					byte lastLager = input.ReadByte();
					if (lastLager < lagerCount)
					{
						LastLager = lastLager;
						// Read list
						Lagers.Capacity = lagerCount;
						for (int i = 0; i < lagerCount; i++)
							Lagers.Add(new Tuple<string, string>(input.ReadString(), input.ReadString()));
					}
				}
			}
		}

		public async Task Save()
		{
			IIoProvider io = Lager.IoProvider;
			BinaryWriter output = await io.WriteFile(SETTINGS_FILE);
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
