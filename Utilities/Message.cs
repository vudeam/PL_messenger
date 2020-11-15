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
		public String Content { get; set; }
		public String FromID { get; set; }
		public DateTime Timestamp { get; set; }

		/// <returns>Formatted string which shows timestamp, user ID and message contents </returns>
		public override String ToString()
		{
			return String.Format(
				"[{0}] {1} : {2}",
				this.Timestamp.ToLongTimeString(),
				this.FromID,
				this.Content
			);
		}

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

		public override Boolean Equals(object obj)
		{
			return obj is Message message &&
				   Content == message.Content &&
				   FromID == message.FromID &&
				   Timestamp == message.Timestamp;
		}
	}
}
