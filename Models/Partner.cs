using Microsoft.WindowsAzure.Storage.Table;

namespace StyleEl.Models
{
	public class Partner : TableEntity
	{
		public string Name { get; set; } = "";
		public string? Url { get; set; }
	}
}