using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;

namespace VectorChat.ServerASPNET.Controllers
{
	/// <remarks>Route: <c>/api/chat</c></remarks>
	[Route("api/[controller]")]
	[ApiController]
	public class ChatController : ControllerBase
	{
		/// <summary>
		/// Triggered by <see cref="VectorChat.ServerASPNET.Controllers.ChatController.PostMessage(Message)"/> 
		/// when a new message is added
		/// </summary>
		private static event MessageEventHandler MessageAdded;
		private static readonly ILogger consoleLogger;
		private static readonly ILogger fileLogger;

		static ChatController()
		{
			MessageAdded = OnMessageAdded;
			fileLogger = new FileLogger(Path.Combine(Directory.GetCurrentDirectory(), "logs", "ChatLog.log"));
			consoleLogger = LoggerFactory.Create(logBuilder =>
			{
				logBuilder.AddConsole();
				logBuilder.AddDebug();
			}).CreateLogger<ChatController>();
		}

		/// <summary>
		/// Get messages in the period from <paramref name="ts"/> to <see cref="DateTime.Now"/><br/>
		/// </summary>
		/// <remarks>Route: <c>GET messages/{nickname}/{userID}/{groupID}/{ts}</c></remarks>
		[HttpGet("messages/{nick}/{uID}/{gID}/{ts}")]
		[Produces("application/json")]
		public async Task<IActionResult> GetMessagesAfterTs(string nick, uint uID, uint gID, DateTime ts)
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

			if (!Server.CheckUserRegistration($"{nick}#{uID}")) return Ok(new List<Message>());

			if (Server.groupsStorage.ContainsKey(gID)) // found requested group
			{
				if (Server.groupsStorage[gID].group.members.Exists(u => u == new User(nick, uID))) // found group member
				{
					DateTime mark = Server.groupsStorage[gID].messages.Count == 0 ? DateTime.Now : Server.groupsStorage[gID].messages.Min(m => m.timestamp);
					// if needed messages are already loaded to a file then load from that file
					if (ts < mark)
					{
						List<Message> reallyAllMessages = Server.LoadGropMessages(gID);

						reallyAllMessages.AddRange(Server.groupsStorage[gID].messages);

						// SELECT messages with Timestamp less than {ts}
						var messages = from msg in reallyAllMessages
									   where msg.timestamp > ts
									   orderby msg.timestamp
									   select msg;

						//response = new List<Message>(messages);
						return await Task.Run(() => Ok(new List<Message>(messages)));
					}
					else
					{
						// SELECT messages with Timestamp less than {ts}
						var messages = from msg in Server.groupsStorage[gID].messages
									   where msg.timestamp > ts
									   orderby msg.timestamp
									   select msg;

						//response = await Task.Run(() => new List<Message>(messages));
						return await Task.Run(() => Ok(new List<Message>(messages)));
					}
				}
			}

			return Ok(new List<Message>());
		}

		/// <summary>
		/// Get <paramref name="count"/> previous messages from <paramref name="ts"/><br/>
		/// </summary>
		/// <remarks>Route: <c>GET messages/{nickname}/{userID}/{groupID}/{ts}/{count}</c></remarks>
		[HttpGet("messages/{nick}/{uID}/{gID}/{ts}/{count}")]
		[Produces("application/json")]
		public async Task<IActionResult> GetCountMessagesBeforeTs(string nick, uint uID, uint gID, DateTime ts, int count)
		{
			#region Logging
			if (Server.config.EnableFileLogging)
			{
				fileLogger.Log(LogLevel.Information, "{0,6} {1} {2}",
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

			if (!Server.CheckUserRegistration($"{nick}#{uID}")) return Ok(new List<Message>());

			if (Server.groupsStorage.ContainsKey(gID)) // found requested group
			{
				if (Server.groupsStorage[gID].group.members.Exists(u => u == new User(nick, uID))) // found group member
				{
					int c = (from msg in Server.groupsStorage[gID].messages
							  where msg.timestamp <= ts
							  orderby msg.timestamp
							  select msg).Count();
					
					// if not enough messages are stored - load from file
					if (c < count)
					{
						List<Message> reallyAllMessages = Server.LoadGropMessages();
						reallyAllMessages.AddRange(Server.groupsStorage[gID].messages);

						var messages = (from msg in reallyAllMessages
										where msg.timestamp < ts
										orderby msg.timestamp
										select msg)
										.TakeLast(count);

						//response = new List<Message>(messages);
						return await Task.Run(() => Ok(new List<Message>(messages)));
					}
					else // act as normal and select from RAM
					{
						var messages = (from msg in Server.groupsStorage[gID].messages
										where msg.timestamp < ts
										orderby msg.timestamp
										select msg)
										.TakeLast(count);

						//response = new List<Message>(messages);
						return await Task.Run(() => Ok(new List<Message>(messages)));
					}
				}
			}

			return Ok(new List<Message>());
		}

		/// <summary>
		/// Get <see cref="System.Collections.Generic.List{VectorChat.Utilities.Message}"/> of all messages in 
		/// <see cref="VectorChat.Utilities.Credentials.Group"/> specified by <paramref name="gID"/>
		/// </summary>
		/// <remarks>Route: <c>GET messages/{gID}</c></remarks>
		[HttpGet("messages/{gID}")]
		[Produces("application/json")]
		public IEnumerable<Message> GetMessagesInGroup(uint gID)
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
			consoleLogger.Log(LogLevel.Information, "{0,6} {1} {2}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path
			);
			#endregion

			if (Server.groupsStorage.ContainsKey(gID)) // registered group exists
			{
				return Server.groupsStorage[gID].messages;
			}

			return new List<Message>();
		}

		/// <summary>
		/// Get list of <see cref="VectorChat.Utilities.Credentials.Group"/> 
		/// which contain specified <see cref="VectorChat.Utilities.Credentials.User"/> as a member
		/// </summary>
		/// <remarks>Route: <c>GET groups/{nick}/{uID}</c></remarks>
		[HttpGet("groups/{nick}/{uID}")]
		[Produces("application/json")]
		public IEnumerable<Group> GetGroupsForUser(string nick, uint uID)
		{
			var response = from g in Server.GroupsList
						   where g.members.Exists(u => u == new User(nick, uID))
						   orderby g.groupID ascending
						   select g;

			return response;
		}

		/// <returns>Number of members in requested <paramref name="gID"/></returns>
		[HttpGet("groups/{nick}/{uID}/{gID}/count")]
		[Produces("application/json")]
		public IActionResult GetGroupMembersCount(string nick, uint uID, uint gID)
		{
			// if provided User of Group does not exist, return 0
			if (!Server.CheckUserRegistration($"{nick}#{uID}") || !Server.groupsStorage.ContainsKey(gID)) return Ok(0);

			return Ok(Server.groupsStorage[gID].group.members.Count);
		}

		/// <summary>
		/// Get and write one <see cref="VectorChat.Utilities.Message"/> from the client
		/// </summary>
		/// <remarks>Route: <c>POST messages</c></remarks>
		/// <param name="msg">Recieved instance of <see cref="VectorChat.Utilities.Message"/></param>
		[HttpPost("messages")]
		[Produces("application/json")]
		public IActionResult PostMessage([FromBody] Message msg)
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
			response.code = ApiErrCodes.GroupUnavailable;
			response.defaultMessage = "Target group is unavailable.";

			if (!Server.CheckUserRegistration(msg.fromID)) // user is not regustered
			{
				response.code = ApiErrCodes.LoginNotFound;
				response.defaultMessage = "Message sender is not a registered User";

				return Ok(response);
			}

			if (Server.groupsStorage.ContainsKey(msg.groupID)) // target group exists
			{
				// recieved notification message
				if (msg.fromID.Equals(MessagePhrases.LoginLogoutNotification))
				{
					Server.groupsStorage[msg.groupID].messages.Add(msg);

					#region Logging
					if (Server.config.EnableFileLogging)
					{
						fileLogger.Log(LogLevel.Information, Server.groupsStorage[msg.groupID].messages[^1].ToString());
					}
					if (Server.config.EnableVerboseConsole)
					{
						consoleLogger.LogInformation(Server.groupsStorage[msg.groupID].messages[^1].ToString());
					}
					#endregion

					// trigger an event in another Thread
					Thread messagesHistoryUpdate = new Thread(OnMessageAdded) { Name = "Updater.exe", IsBackground = true };
					messagesHistoryUpdate.Start();

					response.code = ApiErrCodes.Success;
					response.defaultMessage = "OK";
					response.usr = new User() { nickname = msg.fromID };
				}
				// user belongs to the target group
				else if (Server.groupsStorage[msg.groupID].group.members.Exists(u => u == new User(msg.fromID)))
				{
					Server.groupsStorage[msg.groupID].messages.Add(msg);

					#region Logging
					if (Server.config.EnableFileLogging)
					{
						fileLogger.Log(LogLevel.Information, Server.groupsStorage[msg.groupID].messages[^1].ToString());
					}
					if (Server.config.EnableVerboseConsole)
					{
						consoleLogger.LogInformation(Server.groupsStorage[msg.groupID].messages[^1].ToString());
					}
					#endregion

					// trigger an event in another Thread
					Thread messagesHistoryUpdate = new Thread(OnMessageAdded) { Name = "Updater.exe", IsBackground = true };
					messagesHistoryUpdate.Start();

					response.code = ApiErrCodes.Success;
					response.defaultMessage = "OK";
					response.usr = new User(msg.fromID);
				}
			}

			return Ok(response);
		}

		/// <remarks>Route: <c>GET welcome</c></remarks>
		[HttpGet("welcome")]
		public IActionResult WelcomeMessage()
		{
			consoleLogger.Log(LogLevel.Information, "{0,6} {1} {2}",
				this.Request.Method,
				this.Response.StatusCode,
				this.Request.Path
			);

			Server.groupsStorage[0U].messages.Add(
				new Message() { content = "Hello", fromID = "o#0", timestamp = DateTime.Now, groupID = 0U }
			);

			MessageAdded();

			return Ok("Hello there, General Kenobi");
		}

		#region Non-Action methods

		/// <summary>
		/// Handle <see cref="VectorChat.Utilities.Message"/> addition event:
		/// Move messages from RAM to files and clear RAM
		/// </summary>
		/// <remarks>This method is <c>[NonAction]</c>, so it can not be called by client.</remarks>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[NonAction]
		private static void OnMessageAdded()
		{
			// if less than a limit
			if ((Server.AllMessagesCount <= Server.config.StoredMessagesLimit) || (Server.config.StoredMessagesLimit < 0)) return;

			// upload all messages to a file
			foreach (var pair in Server.groupsStorage)
			{
				List<Message> storedMessages = FileWorker.LoadFromFile<List<Message>>(
					Path.Combine(Directory.GetCurrentDirectory(), "MessagesStorage", $"groupID{pair.Key}", "messages.json")
				);

				// add current messages to already stored
				storedMessages.AddRange(pair.Value.messages);

				FileWorker.SaveToFile(
					Path.Combine(Directory.GetCurrentDirectory(), "MessagesStorage", $"groupID{pair.Key}", "messages.json"),
					storedMessages
				);

				// clear messages stored in Server
				Server.groupsStorage[pair.Key].messages.Clear();
			}

		}

		#endregion
	}
}
