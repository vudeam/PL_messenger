using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VectorChat.Utilities;

namespace VectorChat.ServerASPNET
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration _Configuration)
		{
			this.Configuration = _Configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.Configure<IdentityOptions>(options =>
			{
				options.SignIn.RequireConfirmedEmail = true;
			});

			return;
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
		public void Configure(
			IApplicationBuilder app,
			IWebHostEnvironment env,
			IHostApplicationLifetime lifetime,
			ILogger<Startup> logger
		)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			lifetime.ApplicationStopping.Register(() => Console.WriteLine("Graceful stop"));

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
				logger.Log(
					LogLevel.Information,
					"{0,6} {1} {2}",
					context.Request.Method,
					context.Response.StatusCode,
					context.Request.Path
				);
			});

			return;
		}
	}
}
