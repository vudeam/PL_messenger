namespace VectorChat.Utilities
{
	public enum ApiErrCodes
	{
		/// <summary>
		/// Code for successful operation.<br/>
		/// The default message for this code is "OK"
		/// </summary>
		Success = 0,

		/// <summary>
		/// <see cref="VectorChat.Utilities.Credentials.Account"/> with the same login already exists. 
		/// Registration is impossible.<br/>
		/// The default message for this code is "Registered account with the same login already exists."
		/// </summary>
		LoginTaken,

		/// <summary>
		/// <see cref="VectorChat.Utilities.Credentials.Account"/> with the provided login is not present among registered accounts. 
		/// Authentication is impossible.<br/>
		/// The default message for this code is "Account with this login does not exist."
		/// </summary>
		LoginNotFound,

		/// <summary>
		/// Password for the provided <see cref="VectorChat.Utilities.Credentials.Account"/> does not match with the registered one. 
		/// Authentication failed.<br/>
		/// The default message for this code is "Incorrect password."
		/// </summary>
		PasswordIncorrect,

		/// <summary>
		/// <see cref="VectorChat.Utilities.Credentials.Group.groupID"/> specified in the 
		/// recieved <see cref="VectorChat.Utilities.Message"/> is not avalable.<br/>
		/// The default message for this code is "Target group is unavailable."
		/// </summary>
		GroupUnavailable,

		/// <summary>
		/// An unknown error occured which can not be described by existing error codes<br/>
		/// The default message for this code is "Unknown error."
		/// </summary>
		Unknown
	}
}
