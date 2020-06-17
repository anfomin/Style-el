using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace StyleEl.Controllers
{
	[Route("[controller]")]
	public class FilesController : Controller
	{
		readonly static Regex _retinaRegex = new Regex(@"@(?<scale>\d)x$");
		readonly ILogger _logger;
		readonly IMemoryCache _cache;
		readonly CloudStorageAccount _storage;

		public FilesController(ILogger<FilesController> logger, IMemoryCache cache, CloudStorageAccount storage)
		{
			_logger = logger;
			_cache = cache;
			_storage = storage;
		}

		[HttpGet("{**path}")]
		public async Task<IActionResult> File(string path, [FromQuery]ImageOptions options)
		{
			// process retina sizing
			string ext = Path.GetExtension(path);
			string pathWithoutExtention = path.Remove(path.Length - ext.Length);
			if (!options.IsEmpty && _retinaRegex.Match(pathWithoutExtention) is Match match && match.Success)
			{
				pathWithoutExtention = pathWithoutExtention.Remove(pathWithoutExtention.Length - match.Length);
				path = pathWithoutExtention + ext;
				options.Size *= int.Parse(match.Groups["scale"].Value);
			}

			string pathFinal = pathWithoutExtention + GetSuffix(options) + ext;
			string? url = await _cache.GetOrCreateAsync($"File:{pathFinal}", async entry =>
			{
				entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
				var container = _storage.CreateCloudBlobClient().GetContainerReference(Consts.FilesContainer);
				var blob = container.GetBlockBlobReference(pathFinal);
				if (!await blob.ExistsAsync())
				{
					if (options.IsEmpty)
					{
						_logger.LogInformation($"File '{pathFinal}' does not exist");
						return null;
					}

					// try resize image
					_logger.LogInformation($"Creating file '{pathFinal}' from '{path}'");
					var source = container.GetBlockBlobReference(path);
					if (!await source.ExistsAsync())
					{
						_logger.LogInformation($"Source file '{path}' does not exist");
						return null;
					}

					try
					{
						using var sourceStream = await source.OpenReadAsync();
						using var resized = Imaging.Resize(sourceStream, options)!;
						using var blobStream = await blob.OpenWriteAsync();
						await resized.CopyToAsync(blobStream);
					}
					catch (FormatException)
					{
						// source file format is not image or not supported
						_logger.LogInformation($"Source file format '{path}' is not image");
						return null;
					}
					blob.Properties.ContentType = source.Properties.ContentType;
					await blob.SetPropertiesAsync();
				}
				return blob.Uri.ToString();
			});

			if (url == null)
				return NotFound();
			return Redirect(url);
		}

		string? GetSuffix(ImageOptions options)
		{
			if (options.IsEmpty)
				return null;

			var sb = new StringBuilder();
			if (options.Width > 0)
				sb.Append("$w=" + options.Width);
			if (options.Height > 0)
				sb.Append("$h=" + options.Height);
			sb.Append("$m=" + options.Mode.ToString().ToLower());
			return sb.ToString();
		}
	}
}