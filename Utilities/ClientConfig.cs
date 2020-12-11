using System;
using System.Collections.Generic;
using System.Text;

namespace VectorChat.Utilities
{
	/// <summary>
	/// This structure must be serialized and written to a file.
	/// </summary>
	[Serializable]
	public struct ClientConfig
	{
		public uint messageRequestTime { get; set; }

		public int mainWindowHeight { get; set; }

		public int mainWindowWidth { get; set; }

		/// <summary>Remote server address (including port)</summary>
		public string serverAddress { get; set; }

		/// <summary>Enables using credentials stored in config.json for authentication</summary>
		public bool enableFileAuth { get; set; }

		public string login { get; set; }

		public string password { get; set; }

	}
}
