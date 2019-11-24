using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace StyleEl.Models
{
	public class Publication : TableEntity
	{
		public DateTime Date { get; set; }
		public string Name { get; set; } = "";
		public string Place { get; set; } = "";
		public string Text { get; set; } = "";
	}
}