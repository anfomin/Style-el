using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using StyleEl.Models;

namespace StyleEl.Pages
{
	[ResponseCache(CacheProfileName = "Default")]
	public class ProjectsModel : PageModel
	{
		readonly IMemoryCache _cache;
		readonly CloudStorageAccount _storage;

		public IList<Project>? List { get; private set; }
		public Project? Item { get; private set; }
		public Publication? Publication { get; private set; }
		public Dictionary<string, string[]> Images { get; private set; } = null!;

		public ProjectsModel(IMemoryCache cache, CloudStorageAccount storage)
		{
			_cache = cache;
			_storage = storage;
		}

		public async Task<IActionResult> OnGetAsync(string key)
		{
			var tableClient = _storage.CreateCloudTableClient();
			var table = tableClient.GetTableReference("Projects");
			if (key == null)
			{
				var query = new TableQuery<Project>().Select(new[] {
					nameof(Project.Name),
					nameof(Project.Order)
				});
				var result = await table.ExecuteQuerySegmentedAsync(query, null);
				List = result.Results
					.OrderByDescending(p => p.Order)
					.ThenBy(p => p.Name)
					.ToArray();
			}
			else
			{
				Item = await table.GetAsync<Project>("", key);
				if (Item == null)
					return NotFound();

				if (Item.PublicationKey != null)
				{
					var publicationsTable = tableClient.GetTableReference("Publications");
					Publication = await publicationsTable.GetAsync<Publication>("", Item.PublicationKey, new[] {
						nameof(Publication.Name),
						nameof(Publication.Place)
					});
				}
			}

			Images = await _cache.GetOrCreateAsync("Projects:Images", GetProjectFilesAsync);
			return Page();
		}

		async Task<Dictionary<string, string[]>> GetProjectFilesAsync(ICacheEntry entry)
		{
			entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

			const string DirName = "projects";
			var nameRegex = new Regex($@"^{DirName}/(?<project>[^/]+)/(?<name>\d+)(?<ext>\.\w+)$", RegexOptions.IgnoreCase);
			var container = _storage.CreateCloudBlobClient().GetContainerReference(Consts.FilesContainer);
			var blobs = await container.ListBlobsSegmentedAsync($"{DirName}/", true, BlobListingDetails.None, null, null, null, null);
			return blobs.Results
				.OfType<CloudBlob>()
				.Select(b => {
					var match = nameRegex.Match(b.Name);
					if (!match.Success)
						return null;
					return new
					{
						Project = match.Groups["project"].Value,
						Index = int.Parse(match.Groups["name"].Value),
						Url = Url.Action("File", "Files", new { path = b.Name })
					};
				})
				.Where(item => item != null)
				.GroupBy(item => item!.Project)
				.ToDictionary(
					g => g.Key,
					g => g
						.OrderBy(item => item!.Index)
						.Select(item => item!.Url)
						.ToArray()
				);
		}
	}
}