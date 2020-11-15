using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
		public static readonly ILogger consoleLogger = LoggerFactory.Create(logBuilder =>
		{
			logBuilder.AddConsole();
			logBuilder.AddDebug();
		}).CreateLogger<AuthController>();

		//public static readonly ILogger fileLogger = new FileLogger(Path.Combine(Directory.GetCurrentDirectory(), "AuthLog.txt"));

		/// <summary>
		/// POST signup
		/// </summary>
		[HttpPost("signup")]
		public AuthResponse Register([FromBody] SignupRequest data)
		{
			consoleLogger.Log(LogLevel.Information, $"user:{data.acc.login} {data.acc.password}, {data.nickname}");

			Server.accounts.Add(data.acc);
			consoleLogger.LogInformation($"New Account: {data.acc}");

			User user = new User() { nickname = data.nickname };
			if (Server.users.Exists(i => i.nickname == data.nickname))
			{
				user.userID = (Server.users.Find(i => i.nickname == data.nickname)).userID + 1;
			}
			else
			{
				user.userID = 1;
			}
			Server.users.Add(user);
			consoleLogger.LogInformation($"Added user: {Server.users[Server.users.Count - 1]}");

			return new AuthResponse()
			{
				code = ApiErrCodes.Success,
				defaultMessage = "OK"
			};
		}

		/// <summary>
		/// POST login
		/// </summary>
		[HttpGet("login")]
		public IActionResult Login()
		{
			return Ok("ok");
		}
	}
}
