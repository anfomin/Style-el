using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace StyleEl
{
	public static class StorageExtensions
	{
		public static async Task<T> GetAsync<T>(this CloudTable table, string partitionKey, string rowKey, IList<string>? columns = null)
			where T : TableEntity
		{
			var result = await table.ExecuteAsync(TableOperation.Retrieve<T>(partitionKey, rowKey, columns?.ToList()));
			return (T)result.Result;
		}
	}
}