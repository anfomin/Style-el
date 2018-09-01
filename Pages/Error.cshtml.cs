using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StyleEl.Pages
{
	[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
	public class ErrorModel : PageModel
	{
		public Exception Error { get; private set; }

		public IActionResult OnGet()
		{
			var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
			if (feature == null)
				return Redirect("/");

			Error = feature.Error;
			return Page();
		}
	}
}