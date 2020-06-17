using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StyleEl.Models;

namespace StyleEl.Controllers
{
	public class SitemapController : Controller
	{
		readonly CloudStorageAccount _storage;

		public SitemapController(CloudStorageAccount storage)
			=> _storage = storage;

		[HttpGet("sitemap.xml")]
		[ResponseCache(CacheProfileName = "Default")]
		public async Task<SitemapResult> Index()
		{
			var sitemap = new Sitemap();
			var client = _storage.CreateCloudTableClient();

			// index page
			var textsTable = client.GetTableReference("PageTexts");
			var indexText = await textsTable.GetAsync<PageText>("", "Index", new[] { nameof(PageText.Timestamp) });
			var partnersTable = client.GetTableReference("Partners");
			var partnersResult = await partnersTable.ExecuteQuerySegmentedAsync(new TableQuery<Partner>().Select(new[] { nameof(Partner.Timestamp) }), null);
			var indexLastMod = partnersResult.Results
				.Select(p => p.Timestamp)
				.Prepend(indexText.Timestamp)
				.Max();
			sitemap.Add(new SitemapUrl(Url, "/Index") { Priority = 1, ChangeFrequency = ChangeFrequency.Monthly, LastMod = indexLastMod.UtcDateTime });

			await FillPageAsync(sitemap, client.GetTableReference("Projects"), "/Projects", 0.8);
			await FillPageAsync(sitemap, client.GetTableReference("Publications"), "/Publications", 0.7);
			sitemap.Add(new SitemapUrl(Url, "/Contacts") { ChangeFrequency = ChangeFrequency.Monthly });
			return new SitemapResult(sitemap);
		}

		async Task FillPageAsync(Sitemap sitemap, CloudTable table, string pageName, double? pagePriority)
		{
			var result = await table.ExecuteQuerySegmentedAsync(new TableQuery().Select(new[] {
				nameof(DynamicTableEntity.Timestamp)
			}), null);
			sitemap.AddRange(result.Results
				.Select(r => new SitemapUrl(Url, pageName, new { key = r.RowKey }) { LastMod = r.Timestamp.UtcDateTime })
				.Prepend(new SitemapUrl(Url, pageName) { Priority = pagePriority, ChangeFrequency = ChangeFrequency.Monthly })
			);
		}
	}
}