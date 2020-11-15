using System;

namespace VectorChat.Utilities.Credentials
{
	public struct User
	{
		public string nickname { get; set; }
		public uint userID { get; set; }
		public override string ToString() => String.Format("{0}#{1}", this.nickname, this.userID);
	}

	public struct Account
	{
		public string login { get; set; }
		public string password { get; set; }
		public override string ToString() => String.Format("{0}:{1}", this.login, this.password);
	}

	public struct SignupRequest
	{
		public Account acc { get; set; }
		public string nickname { get; set; }
	}

	public struct AuthResponse
	{
		public ApiErrCodes code { get; set; }
		public string defaultMessage { get; set; }
		public Account? acc { get; set; }
		public User? usr { get; set; }
		public string token { get; set; }
		public override string ToString()
		{
			return string.Format(
				"Code {0} ('{1}')" + Environment.NewLine + "Account {2}:{3}" + Environment.NewLine + "User {4}#{5}" + Environment.NewLine + "Token: {6}",
				this.code,
				this.defaultMessage,
				this.acc.HasValue ? this.acc.Value.login : String.Empty,
				this.acc.HasValue ? this.acc.Value.password : String.Empty,
				this.usr.HasValue ? this.usr.Value.nickname : String.Empty,
				this.usr.HasValue ? this.usr.Value.userID.ToString() : String.Empty,
				this.token
			);
		}
	}
}
