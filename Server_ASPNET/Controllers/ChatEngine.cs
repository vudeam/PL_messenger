using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Server_ASPNET.Controllers
{
	/// <summary>
	/// Route: /api
	/// </summary>
	[Route("api")]
	[ApiController]
	public class ChatEngine : ControllerBase
	{
		public static readonly ILogger logger = LoggerFactory.Create(logBuilder =>
		{
			logBuilder.AddConsole();
			logBuilder.AddDebug();
		}).CreateLogger<ChatEngine>();

		public static List<Message> Messages = new List<Message>();

		/// <summary>
		/// Route: GET /api/messages
		/// </summary>
		[HttpGet("messages")]
		[Produces("application/json")]
		public IActionResult GetAllMessages()
		{
			logger.Log(
				LogLevel.Information,
				"{0,6} {1} {2}, output {3} message(-s)",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path,
				Messages.Count
			);

			return Ok(JsonSerializer.Serialize(Messages));
		}

		/// <summary>
		/// POST /api/messages
		/// </summary>
		[HttpPost("messages")]
		public IActionResult PostMessage([FromBody] Message IncomingMessage)
		{
			Messages.Add(IncomingMessage);

			logger.Log(
				LogLevel.Information,
				"{0,6} {1} {2}, got Message: {3}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path,
				Messages[Messages.Count - 1].ToString()
			);

			return Ok(Messages.Count);
		}

		/// <summary>
		/// Route: GET /api/welcome
		/// </summary>
		[HttpGet("welcome")]
		public IActionResult WelcomeMessage()
		{
			logger.Log(
				LogLevel.Information,
				"{0,6} {1} {2}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path
			);

			Messages.Add(new Message() { Content = "Hello", FromID = "id0", Timestamp = DateTime.Now });

			return Ok("Hello there, General Kenobi");
		}

		/// <summary>
		/// Route: GET /api/welcome/{_name}
		/// </summary>
		[HttpGet("welcome/{_name}")]
		[Produces("application/json")]
		public IActionResult WelcomeMessage(String _name)
		{
			logger.Log(
				LogLevel.Information,
				"{0,6} {1} {2}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path
			);

			return Ok(JsonSerializer.Serialize($"Welcome, {_name}"));
		}

		/// <summary>
		/// Route: POST /api/reauestBody
		/// </summary>
		[HttpPost("requestBody")]
		public IActionResult OutputRequest([FromBody] int value)
		{
			logger.Log(
				LogLevel.Information,
				"{0,6} {1} {2} :{3}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path,
				value.ToString()
			);

			return Ok(value.ToString());
		}
	}
}
