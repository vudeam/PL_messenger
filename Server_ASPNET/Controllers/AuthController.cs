using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;

namespace VectorChat.ServerASPNET.Controllers
{
	/// <summary>
	/// Route: <c>/api/auth</c>
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

		public static readonly ILogger fileLogger = new FileLogger(Path.Combine(Directory.GetCurrentDirectory(), "logs", "AuthLog.log"));

		private static PasswordHasher<Account> hasher = new PasswordHasher<Account>();

		private static readonly string[] welcomes = new string[]
		{
			"is here",
			"is here with us",
			"joined the group",
			"joined us",
			"joined the chat",
			"enters here",
			"lands in this chat",
			"appeared"
		};

		private static readonly Random rng = new Random(DateTime.Now.Millisecond);

		/// <remarks>Route: <c>POST signup</c></remarks>
		[HttpPost("signup")]
		[Produces("application/json")]
		public IActionResult Register([FromBody] SignupRequest data)
		{
			#region Logging
			if (Server.config.EnableFileLogging)
			{
				fileLogger.Log(LogLevel.Information, "[{0}] {1,6} {2} {3}",
						DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy"),
						this.Request.Method,
						this.Response.StatusCode,
						this.Request.Path
				);
			}
			consoleLogger.Log(LogLevel.Debug, "{0,6} {1} {2}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path
			);
			#endregion

			AuthResponse response = new AuthResponse();

			if (Server.usersStorage.ContainsKey(data.acc.login)) // user already registered and tries again
			{
				response.code = ApiErrCodes.LoginTaken;
				response.defaultMessage = "Registered account with the same login already exists.";
			}
			else
			{
				if (Server.usersStorage.TryAdd(
					data.acc.login,
					(
						hasher.HashPassword(data.acc, data.acc.password),
						new User($"{data.nickname}#{ProvideUserID(Server.UsersList, data.nickname)}")
					)
				))
				{
					response.code = ApiErrCodes.Success;
					response.defaultMessage = "OK";

					Server.usersStorage[data.acc.login].user.groupsIDs.Add(0U); // bind group membership on User side
					response.usr = Server.usersStorage[data.acc.login].user;
					response.token = BitConverter.ToString(ComputeHash(response.usr.ToString(), Encoding.UTF8)).Replace("-", String.Empty); // experimental as hash is not used by client


					Server.groupsStorage[0U].group.members.Add(response.usr); // add registered user to the group chat
					//Server.groups.Find(g => g.groupID == 0U).members.Add(response.usr); 1

					consoleLogger.Log(LogLevel.Information, $"Added new User: {response.usr}");
					consoleLogger.Log(LogLevel.Debug, $"{data.acc.login}:{Server.usersStorage[data.acc.login].passHash}{Environment.NewLine}");

					// update data stored in file
					FileWorker.SaveToFileAsync(
						Path.Combine(Directory.GetCurrentDirectory(), "UsersStorage.json"),
						Server.usersStorage
					);
				}
				else
				{
					response.code = ApiErrCodes.Unknown;
					response.defaultMessage = "Unknown error. Possible registration problem";
				}
			}
			Console.WriteLine("response sent");
			return Ok(response);
		}

		/// <remarks>Route: <c>POST login</c></remarks>
		[HttpPost("login")]
		[Produces("application/json")]
		public IActionResult Login([FromBody] SignupRequest data)
		{
			#region Logging
			if (Server.config.EnableFileLogging)
			{
				fileLogger.Log(LogLevel.Information, "[{0}] {1,6} {2} {3}",
						DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy"),
						this.Request.Method,
						this.Response.StatusCode,
						this.Request.Path
				);
			}
			consoleLogger.Log(LogLevel.Debug, "{0,6} {1} {2}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path
			);
			#endregion

			AuthResponse response = new AuthResponse();

			if (Server.usersStorage.ContainsKey(data.acc.login)) // found registered user
			{
				response.code = ApiErrCodes.PasswordIncorrect;
				response.defaultMessage = "Incorrect password";
				if (hasher.VerifyHashedPassword(data.acc, Server.usersStorage[data.acc.login].passHash, data.acc.password) == PasswordVerificationResult.Success) // password hash verified
				{
					response.code = ApiErrCodes.Success;
					response.defaultMessage = "OK";
					response.usr = Server.usersStorage[data.acc.login].user;
					response.token = BitConverter.ToString(ComputeHash(response.usr.ToString(), Encoding.UTF8)).Replace("-", String.Empty); // experimental as hash is not used by client

					// add new Message to notify users about login
					Server.groupsStorage[0U].messages.Add(
						new Message()
						{
							content = $"{response.usr} {welcomes[rng.Next(welcomes.Length)]}",
							// fromID = response.usr.ToString(),
							fromID = Message.LoginNotification,
							groupID = 0U,
							timestamp = DateTime.Now
						}
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

		#region Non-Action methods

		/// <summary>
		/// Compute SHA512 hash from <paramref name="input"/>
		/// </summary>
		/// <remarks>This method is <c>[NonAction]</c>, so it can not be called by client.</remarks>
		/// <param name="input">The string to compute hash from</param>
		/// <param name="encoding">Encoding of the provided string</param>
		[NonAction]
		private byte[] ComputeHash(string input, Encoding encoding)
		{
			return (SHA256.Create()).ComputeHash(encoding.GetBytes(input));
		}

		/// <summary>
		/// Provides unique userID for the <paramref name="desiredNickname"/>. Does the selection from <paramref name="_users"/>.
		/// </summary>
		/// <remarks>This method is <c>[NonAction]</c>, so it can not be called by client.</remarks>
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

		#endregion
	}
}
