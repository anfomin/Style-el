using Microsoft.WindowsAzure.Storage.Table;

namespace StyleEl.Models
{
	public class PageText: TableEntity
	{
		public string Text { get; set; }
	}
}