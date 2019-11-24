using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace StyleEl.Services
{
	public class RedirectFromWwwRule : IRule
	{
		const string Prefix = "www.";
		readonly int _statusCode;
		readonly bool _https;

		public RedirectFromWwwRule(int statusCode, bool https = false)
			=> (_statusCode, _https) = (statusCode, https);

		public void ApplyRule(RewriteContext context)
		{
			var request = context.HttpContext.Request;
			var response = context.HttpContext.Response;
			if (!request.Host.Value.StartsWith(Prefix))
				return;

			var newHost = new HostString(request.Host.Value[Prefix.Length..]);
			var newUrl = UriHelper.BuildAbsolute(
				_https ? "https" : request.Scheme,
				newHost,
				request.PathBase,
				request.Path,
				request.QueryString);
			response.StatusCode = _statusCode;
			response.Headers[HeaderNames.Location] = newUrl;

			context.Result = RuleResult.EndResponse;
			context.Logger.LogInformation($"Redirected to non-www{(_https ? " HTTPS" : "}")}");
		}
	}
}