using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace VectorChat.Utilities
{
	public class FileLogger : ILogger
	{
		private string logFilePath;
		private static object _locker = new object(); // placeholder for lock

		public FileLogger(String _logFilePath)
		{
			this.logFilePath = _logFilePath;
		}

		public IDisposable BeginScope<TState>(TState state) { return null; } // because it does not matter

		public bool IsEnabled(LogLevel level) { return true; } // always available

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception ex, Func<TState, Exception, string> formatter)
		{
			string logPrefix = "";
			switch (logLevel)
			{
				case LogLevel.None:
					break;
				case LogLevel.Critical:
					logPrefix = "[CRIT]";
					break;
				case LogLevel.Error:
					logPrefix = "[EROR]";
					break;
				case LogLevel.Warning:
					logPrefix = "[WARN]";
					break;
				case LogLevel.Information:
					logPrefix = "[INFO]";
					break;
				default:
					logPrefix = String.Empty;
					break;
			}
			if (formatter != null)
			{
				lock (_locker)
				{
					File.AppendAllText(this.logFilePath, $"{logPrefix} {formatter(state, ex)}{Environment.NewLine}", Encoding.UTF8);
				}
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
