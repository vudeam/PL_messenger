using System;

namespace VectorChat.Utilities
{
	public struct Message
	{
		public String Content { get; set; }
		public String FromID { get; set; }
		public DateTime Timestamp { get; set; }

		public override String ToString()
		{
			return String.Format(
				"[{0}] {1} : {2}",
				this.Timestamp.ToLongTimeString(),
				this.FromID,
				this.Content
			);
		}
	}
}
