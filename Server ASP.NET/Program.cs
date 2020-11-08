using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Server_ASP.NET
{
	public struct Message
	{
		public String Content { get; set; }
		public String FromID { get; set; }
		public DateTime Timestamp { get; set; }

		public override String ToString()
		{
			return String.Format(
				"[{0}] {1} : {2}",
				this.Timestamp.ToLongTimeString(),
				this.FromID,
				this.Content
			);
		}
	}

	public class Server
	{
		public static void Main(String[] args)
		{
			CreateHostBuilder(args).Build().Run();
			Console.WriteLine("\nPress Enter to close this window...");
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
