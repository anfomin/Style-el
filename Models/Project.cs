using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace StyleEl.Models
{
	public class Project : TableEntity
	{
		public string Name { get; set; }
		public string Text { get; set; }
		public string Review { get; set; }
		public string PublicationKey { get; set; }
		public int Order { get; set; }
	}
}