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

		/*
		/// <summary>
		/// Period (in seconds) of saving server data (i.e. 
		/// <see cref="VectorChat.Utilities.Message"/> and <see cref="VectorChat.Utilities.Credentials.User"/> lists)
		/// to files
		/// </summary>
		public uint DataLoadSeconds { get; set; }
		*/

		/// <summary>
		/// Choose whether to create log files or not
		/// </summary>
		public bool EnableFileLogging { get; set; }
	}
}
