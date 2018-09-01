using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Rewrite;
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
				.SetCompatibilityVersion(CompatibilityVersion.Latest)
				.AddRazorPagesOptions(ConfigureRazorPages);
			services.AddRouting(opt => opt.LowercaseUrls = true);
			services.AddResponseCaching();
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
				// app.UseHsts();
			}

			// app.UseHttpsRedirection();
			app.UseRewriter(new RewriteOptions()
				.AddRedirect("^index$", "/")
				.AddRedirect("^(.+)/$", "$1"));
			app.UseStaticFiles();
			app.UseResponseCaching();
			app.UseMvc();
		}

		void ConfigureMvc(MvcOptions options)
		{
			options.CacheProfiles.Add("Default", new CacheProfile { Location = ResponseCacheLocation.Any, Duration = 10 * 60 });
		}

		void ConfigureRazorPages(RazorPagesOptions options)
		{
			options.Conventions.ConfigureFilter(new RazorResponseCacheFilter("Default"));
		}
	}
}