using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Zeltlager;
using Zeltlager.Client;

namespace UnitTests
{
	public class MemoryIoProvider : IIoProvider
	{
		class Folder
		{
			internal Dictionary<string, Folder> folders = new Dictionary<string, Folder>();
			internal Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
		}

		class CallbackMemoryStream : MemoryStream
		{
			readonly Action<CallbackMemoryStream> disposeAction;

			public CallbackMemoryStream(Action<CallbackMemoryStream> dp)
			{
				disposeAction = dp;
			}

			public CallbackMemoryStream(Action<CallbackMemoryStream> dp, byte[] content) : base(content)
			{
				disposeAction = dp;
			}

			protected override void Dispose(bool disposing)
			{
				disposeAction(this);
				base.Dispose(disposing);
			}
		}

		static readonly Task nop = Task.WhenAll();

		Folder root = new Folder();

		Folder GetFolder(IEnumerable<string> path)
		{
			var cur = root;
			foreach (var part in path)
				cur = cur.folders[part];
			return cur;
		}

		Folder GetFolder(string path) => GetFolder(IoProvider.GetParts(path));

		public Task<Tuple<string, FileType>[]> ListContents(string path)
		{
			var folder = GetFolder(path);
			return Task.FromResult(folder.folders.Select(f => new Tuple<string, FileType>(f.Key, FileType.Folder)).Concat(
				folder.files.Select(f => new Tuple<string, FileType>(f.Key, FileType.File))).ToArray());
		}

		public Task CreateFolder(string path)
		{
			var parts = IoProvider.GetParts(path);
			var folder = GetFolder(parts.Take(parts.Length - 1));
			if (!folder.folders.ContainsKey(parts.Last()))
				folder.folders[parts.Last()] = new Folder();
			return nop;
		}

		public Task<bool> ExistsFile(string path)
		{
			var parts = IoProvider.GetParts(path);
			var folder = GetFolder(parts.Take(parts.Length - 1));
			return Task.FromResult(folder.files.ContainsKey(parts.Last()));
		}

		public Task<bool> ExistsFolder(string path)
		{
			var parts = IoProvider.GetParts(path);
			var folder = GetFolder(parts.Take(parts.Length - 1));
			return Task.FromResult(folder.folders.ContainsKey(parts.Last()));
		}

		public Task<Stream> ReadFile(string path)
		{
			var parts = IoProvider.GetParts(path);
			var folder = GetFolder(parts.Take(parts.Length - 1));
			return Task.FromResult<Stream>(new MemoryStream(folder.files[parts.Last()]));
		}

		public Task<Stream> WriteFile(string path)
		{
			var parts = IoProvider.GetParts(path);
			var folder = GetFolder(parts.Take(parts.Length - 1));
			return Task.FromResult<Stream>(new CallbackMemoryStream(
				mem => folder.files[parts.Last()] = mem.ToArray()));
		}

		public Task<Stream> AppendFile(string path)
		{
			var parts = IoProvider.GetParts(path);
			var folder = GetFolder(parts.Take(parts.Length - 1));
			byte[] content = new byte[0];
			if (folder.files.ContainsKey(parts.Last()))
				content = folder.files[parts.Last()];
			return Task.FromResult<Stream>(new CallbackMemoryStream(mem =>
			{
				if (!folder.files.ContainsKey(parts.Last()))
					folder.files[parts.Last()] = mem.ToArray();
				else
					folder.files[parts.Last()] = folder.files[parts.Last()].Concat(mem.ToArray()).ToArray();
			}));
		}
	}
}
