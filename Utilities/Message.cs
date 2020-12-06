using System;
using System.Reflection;

namespace VectorChat.Utilities
{
	/// <summary>
	/// Basic struct which represents a single message. This struct is <c>Serializable</c>
	/// </summary>
	[Serializable]
	public struct Message
	{
		public string content { get; set; }
		public DateTime timestamp { get; set; }
		public string fromID { get; set; }
		public uint groupID { get; set; }

		[NonSerialized]
		public static readonly string LoginLogoutNotification = "VectorChat.LoginLogoutNotification";

		/// <returns>Formatted string which shows timestamp, user ID and message contents </returns>
		public override string ToString() => $"[{this.timestamp.ToLongTimeString()}] {this.fromID} : {this.content}";

		public override int GetHashCode()
		{
			int hashBase = 15478363;
			unchecked
			{
				foreach (PropertyInfo info in this.GetType().GetProperties())
					hashBase = hashBase * 486187739 + (info.GetValue(this)).GetHashCode();

				return hashBase;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is Message message &&
				   content == message.content &&
				   fromID == message.fromID &&
				   timestamp == message.timestamp;
		}
	}

	public delegate void MessageEventHandler();

	/// <summary>
	/// Arguments for all <see cref="VectorChat.Utilities.Message"/> events
	/// </summary>
	public class MessageEventArgs
	{
		public int TotalMessagesCount { get; set; }
	}
}
