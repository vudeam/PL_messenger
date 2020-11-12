using System;
using System.Collections.Generic;
using System.Text;

namespace VectorChat.Utilities
{
	public struct config
	{
		public uint messageRequestTime { get; set; }
		public string ServerAddress { get; set; }
		public config(uint _messageRequestTime, string _ServerAdress) {
			messageRequestTime = _messageRequestTime;
			ServerAddress = _ServerAdress;
		}
	}
}
