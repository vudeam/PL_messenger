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
		public ClientConfig(uint messageRequestTime, string serverAddress, int mainWindowHeight, int mainWindowWidth)
		{
			this.messageRequestTime = messageRequestTime;
			this.serverAddress = serverAddress;
			this.mainWindowHeight = mainWindowHeight;
			this.mainWindowWidth = mainWindowWidth;
		}

		public uint messageRequestTime { get; set; }
		public string serverAddress { get; set; }
		public int mainWindowHeight { get; set; }
		public int mainWindowWidth { get; set; }

	}
}
