using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StyleEl.Models;

namespace StyleEl.Pages
{
	public class PublicationsModel : PageModel
	{
		readonly CloudStorageAccount _storage;

		public IList<Publication> List { get; private set; }
		public Publication Item { get; private set; }

		public PublicationsModel(CloudStorageAccount storage)
		{
			_storage = storage;
		}

		public async Task<IActionResult> OnGetAsync(string key)
		{
			var tableClient = _storage.CreateCloudTableClient();
			var table = tableClient.GetTableReference("Publications");
			if (key == null)
			{
				var query = new TableQuery<Publication>().Select(new[] {
					nameof(Publication.Date),
					nameof(Publication.Name),
					nameof(Publication.Place)
				});
				var result = await table.ExecuteQuerySegmentedAsync(query, null);
				List = result.Results
					.OrderByDescending(p => p.Date)
					.ToArray();
			}
			else
			{
				Item = await table.GetAsync<Publication>("", key);
				if (Item == null)
					return NotFound();
			}
			return Page();
		}
	}
}