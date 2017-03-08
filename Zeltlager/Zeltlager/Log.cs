using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;

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
			const string TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

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
				return string.Format("{0}|{1}|{2}|{3}", Timestamp.ToString(TIME_FORMAT), Type, Escape(Section).Replace("|", " "), Escape(Text));
			}

			static string Escape(string s)
			{
				return s.Replace("\n", "    ").Replace("\r", "");
			}
		}

		const string FILENAME = "log.txt";

		public delegate void OnMessageHandler(Message message);

		public event OnMessageHandler OnMessage;

		readonly List<Message> messages = new List<Message>();
		readonly IIoProvider ioProvider;

		public IReadOnlyCollection<Message> Messages => messages;

		public Log(IIoProvider io)
		{
			ioProvider = io;
		}

		public async Task Load()
		{
			if (!await ioProvider.ExistsFile(FILENAME))
				return;

			using (var file = new StreamReader(await ioProvider.ReadFile(FILENAME)))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					try
					{
						// Parse log line
						var parts = line.Split(new char[] { '|' }, 4);
						messages.Add(new Message(DateTime.Parse(parts[0]), (LogType)Enum.Parse(typeof(LogType), parts[1]), parts[2], parts[3]));
					} catch (Exception)
					{
						messages.Add(new Message(LogType.Error, "", line));
					}
				}
			}
			await Info("Log", "Loaded the log");
		}

		public Task Info(string section, string message) => AddMessage(new Message(LogType.Info, section, message));
		public Task Warning(string section, string message) => AddMessage(new Message(LogType.Warning, section, message));
		public Task Error(string section, string message) => AddMessage(new Message(LogType.Error, section, message));
		public Task Exception(string section, Exception e) => AddMessage(new Message(LogType.Exception, section, e.ToString()));

		/// <summary>
		/// Clear all messages from the log.
		/// </summary>
		public async Task Clear()
		{
			messages.Clear();

			// Clear the log file
			if (!await ioProvider.ExistsFile(FILENAME))
				return;

			using (var file = await ioProvider.WriteFile(FILENAME))
			{ }
		}

		/// <summary>
		/// Add a log message and write it to the log file.
		/// </summary>
		/// <param name="message">The message that should be saved.</param>
		async Task AddMessage(Message message)
		{
			messages.Add(message);
			OnMessage?.Invoke(message);
			try
			{
				using (StreamWriter writer = new StreamWriter(await ioProvider.AppendFile(FILENAME)))
					writer.WriteLine(message);
			}
			catch
			{
				messages.Add(new Message(LogType.Error, "Log", "Can't write log"));
				OnMessage?.Invoke(message);
			}
		}

		/// <summary>
		/// Format the whole log into one string.
		/// </summary>
		/// <returns>A string containing the log.</returns>
		public string Print()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var message in messages)
				sb.AppendLine(message.ToString());
			return sb.ToString();
		}

		/// <summary>
		/// Format selected messages of the log into a string.
		/// </summary>
		/// <param name="printInfo">True, if info messages should appear in the returned message.</param>
		/// <param name="printWarning">True, if warnings should appear in the returned message.</param>
		/// <param name="printError">True, if errors should appear in the returned message.</param>
		/// <param name="printException">True, if exceptions should appear in the returned message.</param>
		/// <returns></returns>
		public string Print(bool printInfo, bool printWarning, bool printError, bool printException)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var message in messages)
			{
				switch (message.Type)
				{
					case LogType.Info:
						if (printInfo)
							sb.AppendLine(message.ToString());
						break;
					case LogType.Warning:
						if (printWarning)
							sb.AppendLine(message.ToString());
						break;
					case LogType.Error:
						if (printError)
							sb.AppendLine(message.ToString());
						break;
					case LogType.Exception:
						if (printException)
							sb.AppendLine(message.ToString());
						break;
				}
			}
			return sb.ToString();
		}
	}
}
