using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace VectorChat.Utilities
{
	public class FileLogger : ILogger
	{
		private String logFilePath;
		private static Object _locker = new Object(); // placeholder for lock

		public FileLogger(String _logFilePath)
		{
			this.logFilePath = _logFilePath;
		}

		public IDisposable BeginScope<TState>(TState state) { return null; } // because it does not matter

		public Boolean IsEnabled(LogLevel level) { return true; } // always available

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception ex, Func<TState, Exception, string> formatter)
		{
			switch (logLevel)
			{
				case LogLevel.None:
					break;
				default:
					if (formatter != null)
					{
						lock (_locker)
						{
							File.AppendAllText(this.logFilePath, formatter(state, ex) + Environment.NewLine, Encoding.UTF8);
						}
					}
					break;
			}
		}
	}

	public class FileLoggerProvider : ILoggerProvider
	{
		private String path;

		public FileLoggerProvider(String _path)
		{
			this.path = _path;
		}

		public ILogger CreateLogger(string categoryName) { return new FileLogger(this.path); }

		public void Dispose() { return; } // because it does not matter
	}

	public static class FileLoggerFactory
	{
		public static ILoggerFactory Create(this ILoggerFactory factory, String logFilePath)
		{
			factory.AddProvider(new FileLoggerProvider(logFilePath));
			return factory;
		}
	}
}
