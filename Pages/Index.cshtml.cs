using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StyleEl.Models;

namespace StyleEl.Pages
{
	public class IndexModel : PageModel
	{
		readonly CloudStorageAccount _storage;

		public string Text { get; private set; }
		public IList<Partner> Partners { get; private set; }

		public IndexModel(CloudStorageAccount storage)
		{
			_storage = storage;
		}

		public async Task OnGetAsync()
		{
			var tableClient = _storage.CreateCloudTableClient();

			var textsTable = tableClient.GetTableReference("PageTexts");
			Text = (await textsTable.GetAsync<PageText>("", "Index")).Text;

			var partnersTable = tableClient.GetTableReference("Partners");
			var partnersResult = await partnersTable.ExecuteQuerySegmentedAsync(new TableQuery<Partner>(), null);
			Partners = partnersResult.Results
				.OrderBy(p => p.Name)
				.ToArray();
		}
	}
}
