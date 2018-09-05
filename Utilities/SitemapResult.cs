using System;
using Microsoft.AspNetCore.Mvc;
using StyleEl.Models;

namespace StyleEl
{
	public class SitemapResult : ActionResult
	{
		public Sitemap Sitemap { get; set; }

		public SitemapResult(Sitemap sitemap)
		{
			Sitemap = sitemap;
		}

		public override void ExecuteResult(ActionContext context)
		{
			if (Sitemap == null)
				throw new NullReferenceException("Sitemap is null");

			var response = context.HttpContext.Response;
			response.ContentType = "application/xml";
			Sitemap.ToStream(response.Body);
		}
	}
}