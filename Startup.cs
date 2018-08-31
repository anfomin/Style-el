using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
			services.AddRouting(opt => opt.LowercaseUrls = true);
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
			app.UseMvc();
		}
	}
}