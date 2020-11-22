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
		private static ServerConfig config;
		//internal static List<Account> accounts;
		internal static List<User> users;
		internal static Dictionary<string, string> accounts;
		internal static Dictionary<string, User> loginUser;

		private static readonly ILogger consoleLogger = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.AddDebug();
		}).CreateLogger<Server>();

		public static void Main(string[] args)
		{
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config.json")))
			{
				config = FileWorker.LoadFromFile<ServerConfig>(Path.Combine(Directory.GetCurrentDirectory(), "config.json"));
			}
			else
			{
				config = new ServerConfig() { Port = "8080" };
				FileWorker.SaveToFile<ServerConfig>(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), new ServerConfig() { Port = "8080" });
			}

			//accounts = new List<Account>();
			users = new List<User>();
			accounts = new Dictionary<string, string>();
			loginUser = new Dictionary<string, User>();

			CreateHostBuilder(args).Build().Run();
			Console.WriteLine(Environment.NewLine + "Press Enter to close this window...");
			Console.ReadLine();

			return;
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
