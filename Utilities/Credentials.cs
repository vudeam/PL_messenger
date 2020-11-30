using System;
using System.Collections.Generic;

namespace VectorChat.Utilities.Credentials
{
	[Serializable]
	public class User
	{
		public string nickname { get; set; }
		public uint userID { get; set; }
		public List<uint> groupsIDs { get; set; } // for client
		public User() { this.groupsIDs = new List<uint>(); }
		public User(string fullName) : this()
		{
			this.nickname = fullName.Split('#', 2)[0];
			this.userID = uint.Parse(fullName.Split('#', 2)[1]);
		}
		/// <returns><c>nickname#userID</c></returns>
		public override string ToString() => $"{this.nickname}#{this.userID}";
		public static bool operator == (User _left, User _right) =>
			(_left.nickname == _right.nickname) && (_right.userID == _left.userID);
		public static bool operator != (User _left, User _right) =>
			(_left.nickname != _right.nickname) || (_left.userID != _right.userID);
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
		public Group(uint _groupID, string _name) : this() { this.groupID = _groupID; this.name = _name; }
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
	public struct AuthResponse
	{
		/// <summary>
		/// <see cref="VectorChat.Utilities.ApiErrCodes"/> code for the request.
		/// </summary>
		/// <remarks>(<c>0</c> - Success)</remarks>
		public ApiErrCodes code { get; set; }

		/// <summary>Default description for the exact error code</summary>
		public string defaultMessage { get; set; }

		/// <summary>Responded <see cref="VectorChat.Utilities.Credentials.User"/> object</summary>
		public User usr { get; set; }

		/// <summary>Token string for (possible) message encryption</summary>
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
