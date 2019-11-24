using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using Microsoft.WindowsAzure.Storage;

namespace StyleEl
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
			=> Configuration = configuration;

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers(ConfigureMvc);
			services.AddRazorPages();
			services.AddRouting(opt => opt.LowercaseUrls = true);
			services.AddResponseCaching();
			services.AddSingleton(CloudStorageAccount.Parse(Configuration.GetConnectionString("Storage")));
			services.AddSingleton<MarkdownProvider>();
			services.Configure<WebEncoderOptions>(opt =>
			{
				opt.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (!env.IsProduction())
				app.UseDeveloperExceptionPage();
			else
				app.UseExceptionHandler("/Error");

			app.UseRequestLocalization(new RequestLocalizationOptions
			{
				DefaultRequestCulture = new RequestCulture("ru-RU")
			});
			app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = PrepareStaticFile });
			app.UseRewriter(ConfigureRewrite(env));
			app.UseResponseCaching();
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapRazorPages();
			});
		}

		RewriteOptions ConfigureRewrite(IWebHostEnvironment env)
		{
			var opt = new RewriteOptions();
			if (env.IsProduction())
				opt.AddRedirectToHttpsPermanent();
			return opt
				.AddRedirect("^index$", "/")
				.AddRedirect("^(.+)/$", "$1");
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