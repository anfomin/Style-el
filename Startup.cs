using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;

namespace StyleEl
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc(ConfigureMvc)
				.SetCompatibilityVersion(CompatibilityVersion.Latest);
			services.AddRouting(opt => opt.LowercaseUrls = true);
			services.AddResponseCaching();
			services.AddHttpsRedirection(opt => opt.HttpsPort = 443);
			services.AddSingleton<IPageApplicationModelProvider, DefaultResponseCacheApplicationModelProvider>();
			services.AddSingleton(CloudStorageAccount.Parse(Configuration.GetConnectionString("Storage")));
			services.AddSingleton<MarkdownProvider>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				app.UseHttpsRedirection();
			}

			app.UseRewriter(new RewriteOptions()
				.AddRedirect("^index$", "/")
				.AddRedirect("^(.+)/$", "$1"));
			app.UseRequestLocalization(new RequestLocalizationOptions
			{
				DefaultRequestCulture = new RequestCulture("ru-RU")
			});
			app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = PrepareStaticFile });
			app.UseResponseCaching();
			app.UseMvc();
		}

		void ConfigureMvc(MvcOptions options)
		{
			options.CacheProfiles.Add("Default", new CacheProfile { Location = ResponseCacheLocation.Any, Duration = 10 * 60 });
		}

		void PrepareStaticFile(StaticFileResponseContext responseContext)
		{
			var headers = responseContext.Context.Response.GetTypedHeaders();
			headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
			{
				Public = true,
				MaxAge = TimeSpan.FromMinutes(10)
			};
		}
	}
}