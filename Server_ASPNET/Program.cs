using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;

namespace VectorChat.ServerASPNET
{
	public class Server
	{
		internal static ServerConfig config;

		/// <summary>login -> (password hash, <see cref="VectorChat.Utilities.Credentials.User"/>)</summary>
		internal static Dictionary<string, (string passHash, User user)> usersStorage;

		/// <summary>groupID -> (<see cref="VectorChat.Utilities.Credentials.Group"/>, List of group messages)</summary>
		internal static Dictionary<uint, (Group group, List<Message> messages)> groupsStorage;

		/// <summary>
		/// Gets List of <see cref="VectorChat.Utilities.Credentials.User"/> 
		/// convereted from <see cref="VectorChat.ServerASPNET.groupsStorage"/>
		/// </summary>
		internal static List<User> UsersList
		{
			get => new List<(string, User)>(usersStorage.Values)
				.ConvertAll(i => i.Item2);
		}

		/// <summary>
		/// Gets or sets List of 
		/// <see cref="VectorChat.Utilities.Credentials.Group"/> 
		/// convereted from <see cref="VectorChat.ServerASPNET.groupsStorage"/>
		/// </summary>
		internal static List<Group> GroupsList
		{
			get => new List<(Group, List<Message>)>(groupsStorage.Values).ConvertAll(i => i.Item1);
			set
			{
				groupsStorage = new Dictionary<uint, (Group, List<Message>)>();
				foreach (var item in value)
				{
					groupsStorage[item.groupID] = (item, new List<Message>());
				}
			}
		}

		/// <summary>
		/// Gets total amount of all items of <see cref="VectorChat.Utilities.Message"/> 
		/// stored in <see cref="VectorChat.ServerASPNET.Server.groupsStorage"/>
		/// </summary>
		internal static int AllMessagesCount
		{
			get => new List<(Group, List<Message>)>(groupsStorage.Values)
				.ConvertAll(i => i.Item2)
				.Sum(list => list.Count);
		}

		private static readonly ILogger consoleLogger = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.AddDebug();
		}).CreateLogger<Server>();

		public static void Main(string[] args)
		{
			try
			{
				consoleLogger.Log(LogLevel.Warning, "Loading config.json ...");
				LoadConfig();
				if (args.Length == 1) config.Port = args[0];

				consoleLogger.Log(LogLevel.Warning, "Loading Users storage ...");
				LoadUsersStorage(Path.Combine(Directory.GetCurrentDirectory(), "usersStorage.json"));

				consoleLogger.Log(LogLevel.Warning, "Loading list of Groups ...");
				if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "groups.json")))
				{
					GroupsList = FileWorker.LoadFromFile<List<Group>>("groups.json");
				}
				else
				{
					groupsStorage = new Dictionary<uint, (Group, List<Message>)>();
					// Hardcode one Group with ID = 0 (gloal group chat)
					groupsStorage.Add(
						0U,
						(new Group(0U, "VectorChat") { isPersonalGroup = false, members = new List<User>() }, new List<Message>())
					);
					//--------------------------------------------------
				}

				consoleLogger.Log(LogLevel.Warning, "Setting up messages storage...");
				Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "MessagesStorage"));
				foreach (uint gID in groupsStorage.Keys)
				{
					string messagesFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "MessagesStorage", $"groupID{gID}");
					Directory.CreateDirectory(messagesFolderPath);
					// create emoty messages storage if not exists
					if (!File.Exists(Path.Combine(messagesFolderPath, "messages.json")))
					{
						FileWorker.SaveToFile(Path.Combine(messagesFolderPath, "messages.json"), new List<Message>());
					}
				}
			}
			catch (Exception ex)
			{
				consoleLogger.Log(LogLevel.Critical, ex, "Failed to load initial files");

				Console.WriteLine(Environment.NewLine + "Press Enter to close this window...");
				Console.ReadLine();
				return;
			}

			CreateHostBuilder(args).Build().Run();

			Console.WriteLine(Environment.NewLine + "Press Enter to close this window...");
			Console.ReadLine();

			return;
		}

		/// <summary>
		/// Checks if specified <paramref name="_usr"/> is registered on the <see cref="VectorChat.ServerASPNET.Server"/> 
		/// (is added to the List of registered Users)
		/// </summary>
		internal static bool CheckUserRegistration(string _usr)
		{
			if (_usr.Equals(MessagePhrases.LoginLogoutNotification)) return true;
			else return UsersList.Exists(u => u == new User(_usr));
		}

		/// <returns>
		/// <see cref="List{VectorChat.Utilities.Message}"/> of Messages from 
		/// <see cref="VectorChat.Utilities.Credentials.Group"/> identified by <paramref name="gID"/>
		/// </returns>
		internal static List<Message> LoadGropMessages(uint gID = 0)
		{
			if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "MessagesStorage", $"groupID{gID}", "messages.json")))
			{
				return FileWorker.LoadFromFile<List<Message>>(
					Path.Combine(Directory.GetCurrentDirectory(), "MessagesStorage", $"groupID{gID}", "messages.json")
				);
			}
			else
			{
				return new List<Message>();
			}
		}

		private static void LoadUsersStorage(string path)
		{
			if (File.Exists(path))
			{
				usersStorage = FileWorker.LoadFromFile<Dictionary<string, (string, User)>>(path);
			}
			else // load failed, reset storage
			{
				usersStorage = new Dictionary<string, (string, User)>();
			}
		}
		
		private static void LoadConfig()
		{
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
			{
				config = FileWorker.LoadFromFile<ServerConfig>(
					Path.Combine(Directory.GetCurrentDirectory(),
					"config.json")
				);
				if (config.EnableFileLogging)
				{
					Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));
				}
			}
			else
			{
				consoleLogger.Log(LogLevel.Warning, "config.json not found. loading default config...");
				config = new ServerConfig() // default config
				{
					Port = "8080",
					EnableFileLogging = false,
					EnableVerboseConsole = false,
					StoredMessagesLimit = 50
				};
				if (config.EnableFileLogging)
				{
					Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));
				}
				FileWorker.SaveToFile(
					Path.Combine(Directory.GetCurrentDirectory(), "config.json"),
					config
				);
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
						.UseKestrel()
						.UseContentRoot(Directory.GetCurrentDirectory())
						.UseUrls(
							$"http://0.0.0.0:{config.Port}"
						)
						.UseStartup<Startup>();
				});
	}
}
