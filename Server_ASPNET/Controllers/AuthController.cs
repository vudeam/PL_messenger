using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;

namespace VectorChat.ServerASPNET.Controllers
{
	/// <summary>
	/// Route: /api/auth
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private static readonly ILogger consoleLogger = LoggerFactory.Create(logBuilder =>
		{
			logBuilder.AddConsole();
			logBuilder.AddDebug();
		}).CreateLogger<AuthController>();

		//public static readonly ILogger fileLogger = new FileLogger(Path.Combine(Directory.GetCurrentDirectory(), "AuthLog.txt"));
		private static PasswordHasher<Account> hasher = new PasswordHasher<Account>();

		/// <summary>
		/// POST signup
		/// </summary>
		[HttpPost("signup")]
		[Produces("application/json")]
		public IActionResult Register([FromBody] SignupRequest data)
		{
			#region Old Implementation
			//consoleLogger.Log(LogLevel.Information, $"user:{data.acc.login} {data.acc.password}, {data.nickname}");
			//Server.accounts.Add(data.acc);
			//consoleLogger.LogInformation($"New Account: {data.acc}");
			//User user = new User() { nickname = data.nickname };
			//if (Server.users.Exists(i => i.nickname == data.nickname))
			//{
			//	user.userID = (Server.users.Find(i => i.nickname == data.nickname)).userID + 1;
			//}
			//else
			//{
			//	user.userID = 1;
			//}
			//Server.users.Add(user);
			//consoleLogger.LogInformation($"Added user: {Server.users[Server.users.Count - 1]}");
			//return new AuthResponse()
			//{
			//	code = ApiErrCodes.Success,
			//	defaultMessage = "OK"
			//};
			#endregion
			AuthResponse response = new AuthResponse();

			if (Server.accounts.ContainsKey(data.acc.login)) // user already registered and tries again
			{
				response.code = ApiErrCodes.LoginTaken;
				response.defaultMessage = "Registered account with the same login already exists.";
			}
			else
			{
				if (Server.accounts.TryAdd(data.acc.login, hasher.HashPassword(data.acc, data.acc.password))){
					response.code = ApiErrCodes.Success;
					response.defaultMessage = "OK.";
					response.usr = new User()
					{
						nickname = data.nickname,
						userID = ProvideUserID(Server.users, data.nickname)
					};
					Server.users.Add(response.usr);
					Server.loginUser.Add(data.acc.login, response.usr);

					Console.WriteLine($"Added new user: {response.usr}");
					Console.WriteLine($"{data.acc.login}:{Server.accounts[data.acc.login]}");
					Console.WriteLine($"{data.acc.login}:{Server.loginUser[data.acc.login]}");
				}
				else
				{
					response.code = ApiErrCodes.Unknown;
					response.defaultMessage = "Unknown error. Possible registration problem.";
				}
			}

			return Ok(response);
		}

		/// <summary>
		/// POST login
		/// </summary>
		[HttpPost("login")]
		[Produces("application/json")]
		public IActionResult Login([FromBody] SignupRequest data)
		{
			AuthResponse response = new AuthResponse();
			if (Server.accounts.ContainsKey(data.acc.login)) // found registered user
			{
				response.code = ApiErrCodes.PasswordIncorrect;
				response.defaultMessage = "Incorrect password";
				if (hasher.VerifyHashedPassword(data.acc, Server.accounts[data.acc.login], data.acc.password) == PasswordVerificationResult.Success){ // hash is correct
					response.code = ApiErrCodes.Success;
					response.defaultMessage = "OK";
					response.usr = Server.loginUser[data.acc.login];

					//SHA512 hashSvc = SHA512.Create();
					//Console.WriteLine(BitConverter.ToString((SHA512.Create()).ComputeHash(Encoding.UTF8.GetBytes(response.usr.ToString() + DateTime.Now.ToString()))).Replace("-", String.Empty));
					Console.WriteLine(
						BitConverter.ToString(ComputeHash(response.usr.ToString() + DateTime.Now.ToString(), Encoding.UTF8))
					);
				}
			}
			else // user is not registered
			{
				response.code = ApiErrCodes.LoginNotFound;
				response.defaultMessage = "Account with this login does not exist.";
			}

			return Ok(response);
		}

		/// <summary>
		/// Compute SHA512 hash from <paramref name="input"/>
		/// </summary>
		/// <param name="input">The string to compute hash from</param>
		/// <param name="encoding">Encoding of the provided string</param>
		[NonAction]
		private byte[] ComputeHash(string input, Encoding encoding)
		{
			return (SHA512.Create()).ComputeHash(encoding.GetBytes(input));
		}

		/// <summary>
		/// Provides unique userID for the <paramref name="desiredNickname"/>. Does the selection from <paramref name="_users"/>.
		/// </summary>
		/// <remarks>This method is <c>[NonAction]</c>, so it can't be called by client.</remarks>
		/// <param name="_users">Collection of users to search for unique nicknames from.</param>
		/// <param name="desiredNickname">The nickname which is searched in <paramref name="_users"/></param>
		/// <returns>Unique userID in the provided collection</returns>
		[NonAction]
		private uint ProvideUserID(List<User> _users, string desiredNickname)
		{
			if (_users.Exists(i => i.nickname == desiredNickname))
			{
				List<User> takenIDs = _users.FindAll(i => i.nickname == desiredNickname);
				takenIDs.Sort((user1, user2) => user1.userID.CompareTo(user2.userID));
				//Console.WriteLine("Sorted users:");
				//foreach (var u in takenIDs) Console.WriteLine(u);

				return takenIDs[^1].userID + 1; // (ID with the biggest value) + 1
			}
			else
			{
				return 0U;
			}
		}
	}
}
