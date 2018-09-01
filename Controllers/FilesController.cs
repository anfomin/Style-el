using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.WindowsAzure.Storage;

namespace StyleEl.Controllers
{
	[Route("[controller]")]
	public class FilesController : Controller
	{
		readonly IMemoryCache _cache;
		readonly CloudStorageAccount _storage;

		public FilesController(IMemoryCache cache, CloudStorageAccount storage)
		{
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		[HttpGet("{name}")]
		public async Task<IActionResult> File(string name)
		{
			string url = await _cache.GetOrCreateAsync($"File:{name}", async entry =>
			{
				entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
				var container = _storage.CreateCloudBlobClient().GetContainerReference("files");
				var blob = container.GetBlobReference(name);
				if (!await blob.ExistsAsync())
					return null;
				return blob.Uri.ToString();
			});
			if (url == null)
				return NotFound();
			return Redirect(url);
		}
	}
}