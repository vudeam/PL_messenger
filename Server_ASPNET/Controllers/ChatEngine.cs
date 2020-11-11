using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using VectorChat.Utilities;

namespace VectorChat.ServerASPNET.Controllers
{
	/// <summary>
	/// Route: /api
	/// </summary>
	[Route("api")]
	[ApiController]
	public class ChatEngine : ControllerBase
	{
		public static readonly ILogger consoleLogger = LoggerFactory.Create(logBuilder =>
		{
			logBuilder.AddConsole();
			logBuilder.AddDebug();
		}).CreateLogger<ChatEngine>();

		//public static readonly ILogger fileLogger = new FileLogger(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"));

		public static List<Message> messages = new List<Message>();

		/// <summary>
		/// Route: GET /api/messages
		/// </summary>
		[HttpGet("messages")]
		[Produces("application/json")]
		public IActionResult GetAllMessages()
		{
			//fileLogger.Log(
			//	LogLevel.Information,
			//	"{0,6} {1} {2}, output {3} message(-s)",
			//	this.Request.Method,
			//	this.Response.StatusCode,
			//	this.Request.Path,
			//	messages.Count
			//);
			consoleLogger.Log(
				LogLevel.Information,
				"{0,6} {1} {2}, output {3} message(-s)",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path,
				messages.Count
			);

			return Ok(JsonSerializer.Serialize(messages));
		}

		/// <summary>
		/// POST /api/messages
		/// </summary>
		[HttpPost("messages")]
		public IActionResult PostMessage([FromBody] Message IncomingMessage)
		{
			messages.Add(IncomingMessage);

			//fileLogger.Log(
			//	LogLevel.Information,
			//	"{0,6} {1} {2}, got Message: {3}",
			//	this.Request.Method,
			//	this.Response.StatusCode,
			//	this.Request.Path,
			//	messages[messages.Count - 1].ToString()
			//);
			consoleLogger.Log(
				LogLevel.Information,
				"{0,6} {1} {2}, got Message: {3}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path,
				messages[messages.Count - 1].ToString()
			);

			return Ok(messages.Count);
		}

		/// <summary>
		/// Route: GET /api/welcome
		/// </summary>
		[HttpGet("welcome")]
		public IActionResult WelcomeMessage()
		{
			//fileLogger.Log(
			//	LogLevel.Information,
			//	"{0,6} {1} {2}",
			//	this.Request.Method,
			//	this.Response.StatusCode,
			//	this.Request.Path
			//);
			consoleLogger.Log(
				LogLevel.Information,
				"{0,6} {1} {2}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path
			);

			messages.Add(new Message() { Content = "Hello", FromID = "id0", Timestamp = DateTime.Now });

			return Ok("Hello there, General Kenobi");
		}

		/// <summary>
		/// Route: GET /api/welcome/{_name}
		/// </summary>
		[HttpGet("welcome/{_name}")]
		[Produces("application/json")]
		public IActionResult WelcomeMessage(String _name)
		{
			//fileLogger.Log(
			//	LogLevel.Information,
			//	"{0,6} {1} {2}",
			//	this.Request.Method,
			//	this.Response.StatusCode,
			//	this.Request.Path
			//);
			consoleLogger.Log(
				LogLevel.Information,
				"{0,6} {1} {2}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path
			);

			messages.Add(new Message() { Content = $"Hello, {_name}", FromID = "id0", Timestamp = DateTime.Now });

			return Ok(JsonSerializer.Serialize($"Welcome, {_name}"));
		}

		/// <summary>
		/// Route: POST /api/reauestBody
		/// </summary>
		[HttpPost("requestBody")]
		public IActionResult OutputRequest([FromBody] int value)
		{
			//fileLogger.Log(
			//	LogLevel.Information,
			//	"{0,6} {1} {2} :{3}",
			//	this.Request.Method,
			//	this.Response.StatusCode,
			//	this.Request.Path,
			//	value.ToString()
			//);
			consoleLogger.Log(
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
