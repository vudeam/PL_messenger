using System;
using System.Collections.Generic;
using System.Text;

namespace VectorChat.Utilities
{
	/// <summary>
	/// This structure must be serialized and written to a file.
	/// </summary>
	[Serializable]
	public struct Config
	{
		public uint messageRequestTime { get; set; }
		public string serverAddress { get; set; }
		public Config(uint _messageRequestTime, string _ServerAdress) {
			messageRequestTime = _messageRequestTime;
			serverAddress = _ServerAdress;
		}
	}
}
