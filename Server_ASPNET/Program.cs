using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;

namespace VectorChat.ServerASPNET
{
	public class Server
	{
		internal static ServerConfig config;
		//internal static List<Account> accounts;
		//internal static List<User> users;
		//internal static Dictionary<string, string> accounts;
		//internal static Dictionary<string, User> loginUser;

		/// <summary>login -> (password hash, <see cref="VectorChat.Utilities.Credentials.User"/>)</summary>
		internal static Dictionary<string, (string passHash, User user)> usersStorage;

		/// <summary>groupID -> (<see cref="VectorChat.Utilities.Credentials.Group"/>, List of group messages)</summary>
		internal static Dictionary<uint, (Group group, List<Message> messages)> groupsStorage;
		//internal static List<Group> groups;

		/// <summary>
		/// Gets List of <see cref="VectorChat.Utilities.Credentials.User"/> convereted from <see cref="VectorChat.ServerASPNET.groupsStorage"/>
		/// </summary>
		internal static List<User> UsersList { get => new List<(string, User)>(usersStorage.Values).ConvertAll(i => i.Item2); }

		private static readonly ILogger consoleLogger = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.AddDebug();
		}).CreateLogger<Server>();

		public static void Main(string[] args)
		{
			LoadConfig();

			usersStorage = new Dictionary<string, (string, User)>();
			groupsStorage = new Dictionary<uint, (Group, List<Message>)>();

			// Hardcode exactly one Group with ID = 0 (gloal group chat)
			groupsStorage.Add(
				0U,
				(new Group(0U, "VectorChat") { isPersonalGroup = false, members = new List<User>() }, new List<Message>())
			);
			// ---------------------------------------------------------

			CreateHostBuilder(args).Build().Run();

			Console.WriteLine(Environment.NewLine + "Press Enter to close this window...");
			Console.ReadLine();

			return;
		}

		/// <summary>
		/// Checks if specified <paramref name="_usr"/> is registered on the <see cref="VectorChat.ServerASPNET.Server"/> 
		/// (is added to the List of registered Users)
		/// </summary>
		internal static bool CheckUserRegistration(User _usr) => UsersList.Exists(u => u == _usr);

		private static void LoadConfig()
		{
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
			{
				config = FileWorker.LoadFromFile<ServerConfig>(
					Path.Combine(Directory.GetCurrentDirectory(),
					"config.json")
				);
			}
			else
			{
				config = new ServerConfig() // default config
				{
					Port = "8080",
					//DataLoadSeconds = 3600,
					EnableFileLogging = false
				};
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
