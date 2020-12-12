using System;

namespace VectorChat.Utilities
{
	/// <summary>
	/// This structure must be serialized and written to a file.
	/// </summary>
	[Serializable]
#pragma warning disable CS0660, CS0661
	public struct ClientConfig
#pragma warning restore CS0660, CS0661
	{
		public uint messageRequestTime { get; set; }

		public double mainWindowHeight { get; set; }

		public double mainWindowWidth { get; set; }

		/// <summary>Remote server address (including port)</summary>
		public string serverAddress { get; set; }

		/// <summary>Enables using credentials stored in config.json for authentication</summary>
		public bool enableFileAuth { get; set; }

		public string login { get; set; }

		public string password { get; set; }

		public static bool operator ==(ClientConfig _left, ClientConfig _right)
		{
			return ((_left.messageRequestTime == _right.messageRequestTime)
				&& (_left.mainWindowHeight == _right.mainWindowHeight)
				&& (_left.mainWindowWidth == _right.mainWindowWidth)
				&& (_left.serverAddress == _right.serverAddress)
				&& (_left.enableFileAuth == _right.enableFileAuth)
				&& (_left.login == _right.login)
				&& (_left.password == _right.password));
		}

		public static bool operator !=(ClientConfig _left, ClientConfig _right)
		{
			return ((_left.messageRequestTime != _right.messageRequestTime)
				|| (_left.mainWindowHeight != _right.mainWindowHeight)
				|| (_left.mainWindowWidth != _right.mainWindowWidth)
				|| (_left.serverAddress != _right.serverAddress)
				|| (_left.enableFileAuth != _right.enableFileAuth)
				|| (_left.login != _right.login)
				|| (_left.password != _right.password));
		}
	}
}
