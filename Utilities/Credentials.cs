using System;

namespace VectorChat.Utilities.Credentials
{
	public class User
	{
		public string nickname { get; set; }
		public uint userID { get; set; }
		public override string ToString() => $"{this.nickname}#{this.userID}";
	}

	public class Account
	{
		public string login { get; set; }
		public string password { get; set; }
		public override string ToString() => $"{this.login}:{this.password}";
	}

	public class SignupRequest
	{
		public Account acc { get; set; }
		public string nickname { get; set; }
	}

	/// <summary>
	/// Sample struct of authentication response. Server authentication responds with <c>AuthResponse</c> serialized in JSON.
	/// </summary>
	/// <remarks>
	/// Consists of:
	/// <list type="number">
	/// <item><see cref="VectorChat.Utilities.ApiErrCodes"/> - API error code (<c>0</c> - Success)</item>
	/// <item>Default description for the exact error code</item>
	/// <item><see cref="VectorChat.Utilities.Credentials.User"/> - responded <c>User</c> object</item>
	/// <item>Token string for (possible) message encryption</item>
	/// </list>
	/// </remarks>
	public struct AuthResponse
	{
		public ApiErrCodes code { get; set; }
		public string defaultMessage { get; set; }
		public User usr { get; set; }
		public string token { get; set; }
		public override string ToString()
		{
			return String.Format(
				"Code {0} ('{1}')" + Environment.NewLine + "User {2}#{3}" + Environment.NewLine + "Token: {5}",
				this.code,
				this.defaultMessage,
				this.usr != null ? this.usr.nickname : String.Empty,
				this.usr != null ? this.usr.userID.ToString() : String.Empty,
				this.token
			);
		}
	}
}
