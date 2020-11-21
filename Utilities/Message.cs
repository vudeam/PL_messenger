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
		public string Content { get; set; }
		public string FromID { get; set; }
		public DateTime Timestamp { get; set; }

		/// <returns>Formatted string which shows timestamp, user ID and message contents </returns>
		public override string ToString() => $"[{this.Timestamp.ToLongTimeString()}] {this.FromID} : {this.Content}";

		public override Int32 GetHashCode()
		{
			Int32 hashBase = 15478363;
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
				   Content == message.Content &&
				   FromID == message.FromID &&
				   Timestamp == message.Timestamp;
		}
	}
}
