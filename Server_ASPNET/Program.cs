using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using VectorChat.Utilities.Credentials;

namespace VectorChat.ServerASPNET
{
	public class Server
	{
		public static List<Account> accounts = new List<Account>();
		public static List<User> users = new List<User>();

		public static void Main(String[] args)
		{
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
							"http://0.0.0.0:5005"
						)
						.UseStartup<Startup>();
				});
	}
}
