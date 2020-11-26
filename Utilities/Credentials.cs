using System;
using System.Collections.Generic;

namespace VectorChat.Utilities.Credentials
{
	public class User
	{
		public string nickname { get; set; }
		public uint userID { get; set; }
		public List<uint> groupsIDs { get; set; }
		public User() { this.groupsIDs = new List<uint>(); }
		public User(string fullName) : this()
		{
			this.nickname = fullName.Split('#', 2)[0];
			this.userID = uint.Parse(fullName.Split('#', 2)[1]);
		}
		/// <returns><c>nickname#userID</c></returns>
		public override string ToString() => $"{this.nickname}#{this.userID}";
	}

	public class Account
	{
		public string login { get; set; }
		public string password { get; set; }
		public Account() { }
		public Account(string fullName)
		{
			this.login = fullName.Split(':', 2)[0];
			this.password = fullName.Split(':', 2)[1];
		}
		/// <returns><c>login:password</c></returns>
		public override string ToString() => $"{this.login}:{this.password}";
	}

	public class SignupRequest
	{
		public Account acc { get; set; }
		public string nickname { get; set; }
	}

	public class Group
	{
		public string name { get; set; }
		public uint groupID { get; set; }
		public List<User> members { get; set; }
		public bool isPersonalGroup { get; set; }
		public Group() { this.members = new List<User>(); }
		public override string ToString()
		{
			string output = this.isPersonalGroup ? "Personal group " : "Group ";
			output += $"{this.name}#{this.groupID} ({this.members.Count} members):{Environment.NewLine}";
			foreach (var item in this.members) output += item.ToString() + Environment.NewLine;
			return output;
		}
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
		public override string ToString() =>
			String.Format(
				"Code {0} ('{1}')" + Environment.NewLine + "User {2}" + Environment.NewLine + "Token: {3}",
				this.code,
				this.defaultMessage,
				this.usr != null ? this.usr.ToString() : String.Empty,
				this.token
			);
	}
}
