namespace VectorChat.Utilities
{
	/// <summary>
	/// Configuration for the Server class
	/// </summary>
	public struct ServerConfig
	{
		/// <summary>
		/// Port which will be used to listening for the connections
		/// </summary>
		public string Port { get; set; }

		/// <summary>
		/// Choose whether to create log files or not
		/// </summary>
		public bool EnableFileLogging { get; set; }

		/// <summary>
		/// Enables additional (logging) messages displayed in Console
		/// </summary>
		public bool EnableVerboseConsole { get; set; }

		/// <summary>
		/// Amount of messages to store in RAM while the server is running (the rest of the messages are stored in files and updated regularly
		/// </summary>
		/// <remarks> The value of -1 disables the limit and forces only RAM storage to be used</remarks>
		public int StoredMessagesLimit { get; set; }
	}
}
