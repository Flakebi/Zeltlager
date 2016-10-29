using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class LagerManager
	{
		protected IIoProvider ioProvider;

		public List<LagerBase> Lagers { get; private set; }

		/// <summary>
		/// Load all lagers.
		/// </summary>
		/// <returns></returns>
		public async Task Load()
		{
			//TODO
			Lagers = new List<LagerBase>();
			// Search folders for lagers
			var folders = await ioProvider.ListContents("");
			uint lagerCount = 0;
			while (folders.Contains(new Tuple<string, FileType>(lagerCount.ToString(), FileType.Folder)))
				lagerCount++;
		}
	}
}
