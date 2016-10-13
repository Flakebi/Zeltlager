using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class Log
	{
		public enum LogType
		{
			Info,
			Warning,
			Error,
			Exception,
		}

		public class Message
		{
			public DateTime Timestamp { get; }
			public LogType Type { get; }
			public string Section { get; }
			public string Text { get; }

			internal Message(DateTime timestamp, LogType type, string section, string text)
			{
				Timestamp = timestamp;
				Type = type;
				Section = section;
				Text = text;
			}

			internal Message(LogType type, string section, string text) : this(DateTime.Now, type, section, text) { }

			public override string ToString()
			{
				return string.Format("{0}|{1}|{2}|{3}", Timestamp, Type, Escape(Section).Replace("|", " "), Escape(Text));
			}

			static string Escape(string s)
			{
				return s.Replace("\n", "    ");
			}
		}

		const string FILENAME = "log.txt";

		List<Message> messages = new List<Message>();

		public IReadOnlyCollection<Message> Messages { get { return messages; } }

		public Log() { }

		public async Task Load()
		{
			if (!await LagerBase.IoProvider.ExistsFile(FILENAME))
				return;

			var file = new StreamReader(await LagerBase.IoProvider.ReadFile(FILENAME));
			string line;
			while ((line = file.ReadLine()) != null)
			{
				try
				{
					// Parse log line
					var parts = line.Split(new char[] { '|' }, 4);
					messages.Add(new Message(DateTime.Parse(parts[0]), (LogType)Enum.Parse(typeof(LogType), parts[1]), parts[2], parts[3]));
				}
				catch (Exception)
				{
					messages.Add(new Message(LogType.Error, "", line));
				}
			}
		}

		public Task Info(string section, string message) => print(new Message(LogType.Info, section, message));
		public Task Warning(string section, string message) => print(new Message(LogType.Warning, section, message));
		public Task Error(string section, string message) => print(new Message(LogType.Error, section, message));
		public Task Exception(string section, Exception e) => print(new Message(LogType.Error, section, e.ToString()));

		async Task print(Message message)
		{
			messages.Add(message);
			using (StreamWriter writer = new StreamWriter(await LagerBase.IoProvider.AppendFile(FILENAME)))
				writer.WriteLine(message);
		}
	}
}
