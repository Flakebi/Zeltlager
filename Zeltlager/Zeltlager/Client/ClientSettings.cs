using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
    using Serialisation;
    
	public class ClientSettings
	{
		const string SETTINGS_FILE = "settings.conf";
        const int VERSION = 0;

        [Serialisation]
		public int LastLager { get; set; }


		/// <summary>
		/// The address of the server for remote games.
		/// </summary>
		[Serialisation(Optional = true)]
		public string ServerAddress { get; set; }

        public async Task Load(IIoProvider io)
		{
			// Set default values
			LastLager = 0;

			if (await io.ExistsFile(SETTINGS_FILE))
			{
				using (BinaryReader input = new BinaryReader(await io.ReadFile(SETTINGS_FILE)))
				{
                    if (input.ReadInt32() == VERSION)
                        await new Serialiser<object>().Read(input, null, this);
				}
			}
		}

		public async Task Save(IIoProvider io)
		{
			using (BinaryWriter output = new BinaryWriter(await io.WriteFile(SETTINGS_FILE)))
			{
				output.Write(VERSION);
                await new Serialiser<object>().Write(output, null, this);
			}
		}
	}
}
