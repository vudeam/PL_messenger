using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VectorChat.Utilities;
using VectorChat.Utilities.Credentials;

namespace VectorChat.ServerASPNET
{
	public class Startup
	{
		private ILogger consoleLogger = LoggerFactory.Create(builder =>
		{
			builder.AddConsole();
			builder.AddDebug();
		}).CreateLogger<Startup>();

		private ILogger fileLogger = new FileLogger(Path.Combine(Directory.GetCurrentDirectory(), "StartupLog.log"));

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.Configure<IdentityOptions>(options =>
			{
				options.SignIn.RequireConfirmedEmail = true;
			});
		}

		/// <summary>
		/// Set:
		/// <list type="bullet">
		/// <item>Usage of development environment</item>
		/// <item>Method called on application stop</item>
		/// <item>Controllers mapping</item>
		/// <item>Handler for default (or incorrect) route request</item>
		/// </list>
		/// </summary>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, ILogger<Startup> logger)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			lifetime.ApplicationStopping.Register(() =>
			{
				if (Server.config.EnableFileLogging)
				{
					consoleLogger.LogWarning("Saving files...");
					//if (Server.users.Count > 0)
					//{
					//	FileWorker.SaveToFile<List<User>>(Path.Combine(Directory.GetCurrentDirectory(), "users.json"), Server.users);
					//	consoleLogger.LogInformation($"Saved {Server.users.Count} User(-s)");
					//}
					//if (Server.accounts.Count > 0)
					//{
					//	FileWorker.SaveToFile<Dictionary<string, string>>(Path.Combine(Directory.GetCurrentDirectory(), "accounts.json"), Server.accounts);
					//	consoleLogger.LogInformation($"Saved {Server.accounts.Count} account(-s)");
					//}
					consoleLogger.LogInformation("Finished saving files");
				}
			});

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				//endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
				endpoints.MapControllers();
			});

			app.Run(async (context) =>
			{
				context.Response.StatusCode = StatusCodes.Status404NotFound;
				await context.Response.WriteAsync("<h1>404 Not Found</h1>");

				#region Logging
				if (Server.config.EnableFileLogging)
				{
					fileLogger.Log(LogLevel.Information, "{0,6} {1} {2}",
						context.Request.Method,
						context.Response.StatusCode,
						context.Request.Path
					);
				}
				logger.Log(
					LogLevel.Debug,
					"{0,6} {1} {2}",
					context.Request.Method,
					context.Response.StatusCode,
					context.Request.Path
				);
				#endregion
			});
		}
	}
}
