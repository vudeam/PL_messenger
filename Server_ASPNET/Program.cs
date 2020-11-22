using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VectorChat.Utilities.Credentials;

namespace VectorChat.ServerASPNET
{
	public class Server
	{
		//internal static List<Account> accounts;
		internal static List<User> users;
		internal static Dictionary<string, string> accounts;
		internal static Dictionary<string, User> loginUser;

		private static readonly ILogger consoleLogger = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.AddDebug();
		}).CreateLogger<Server>();

		public static void Main(String[] args)
		{
			//accounts = new List<Account>();
			users = new List<User>();
			accounts = new Dictionary<string, string>();
			loginUser = new Dictionary<string, User>();

			CreateHostBuilder(args).Build().Run();
			Console.WriteLine(Environment.NewLine + "Press Enter to close this window...");
			Console.ReadLine();

			return;
		}

		public static IHostBuilder CreateHostBuilder(String[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
						.UseKestrel()
						.UseContentRoot(Directory.GetCurrentDirectory())
						.UseUrls(
							"http://0.0.0.0:8080"
						)
						.UseStartup<Startup>();
				});
	}
}
