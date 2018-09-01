using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StyleEl
{
	public class DefaultResponseCacheApplicationModelProvider : IPageApplicationModelProvider
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly MvcOptions _mvcOptions;

		public int Order => 0;

		public DefaultResponseCacheApplicationModelProvider(ILoggerFactory loggerFactory, IOptions<MvcOptions> mvcOptions)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_mvcOptions = mvcOptions.Value;
		}

		public void OnProvidersExecuting(PageApplicationModelProviderContext context)
		{
			if (!_mvcOptions.CacheProfiles.TryGetValue("Default", out var cacheProfile))
				throw new InvalidOperationException("Cache profile 'Default' not found");

			var model = context.PageApplicationModel;
			if (!model.Filters.OfType<ResponseCacheFilter>().Any())
				model.Filters.Add(new ResponseCacheFilter(cacheProfile, _loggerFactory));
		}

		public void OnProvidersExecuted(PageApplicationModelProviderContext context) { }
	}
}