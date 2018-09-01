using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StyleEl
{
	public class RazorResponseCacheFilter : IFilterMetadata, IPageFilter
	{
		readonly string _cacheProfileName;

		public RazorResponseCacheFilter(string cacheProfileName)
		{
			_cacheProfileName = cacheProfileName ?? throw new ArgumentNullException(nameof(cacheProfileName));
		}

		public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }

		public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
		{
			var services = context.HttpContext.RequestServices;
			var logger = services.GetRequiredService<ILogger<RazorResponseCacheFilter>>();
			var options = services.GetRequiredService<IOptions<MvcOptions>>().Value;
			if (!options.CacheProfiles.TryGetValue(_cacheProfileName, out var cacheProfile))
				throw new InvalidOperationException($"Cache profile '{_cacheProfileName}' not found");

			var executor = new ResponseCacheFilterExecutor(cacheProfile);
			executor.Execute(context);
			logger.LogInformation(0, $"Executed cache profile '{_cacheProfileName}'");
		}

		public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
	}
}