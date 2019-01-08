using System;
using System.Drawing;
using SkiaSharp;

namespace StyleEl
{
	/// <summary>
	/// Provides extension methods for SkiaSharp imaging.
	/// </summary>
	public static class SkiaSharpExtensions
	{
		public static SKData Encode(this SKBitmap bitmap, SKEncodedImageFormat format, int quality = 80)
		{
			using (var image = SKImage.FromBitmap(bitmap))
				return image.Encode(format, quality);
		}

		/// <summary>
		/// Encodes bitmap into specified format. When target format is JPEG then uses white background color.
		/// </summary>
		public static SKData EncodeDefaultWhite(this SKBitmap bitmap, SKEncodedImageFormat format, int quality = 80)
		{
			// for jpeg we need to render image on white background
			if (format == SKEncodedImageFormat.Jpeg)
			{
				using (var temp = new SKBitmap(bitmap.Width, bitmap.Height, true))
				using (var canvas = new SKCanvas(temp))
				{
					canvas.Clear(SKColors.White);
					canvas.DrawBitmap(bitmap, 0, 0);
					return temp.Encode(format, quality);
				}
			}
			return bitmap.Encode(format, quality);
		}

		/// <summary>
		/// Decodes image from codec with RGBA8888 or BGRA8888 color type.
		/// </summary>
		public static SKBitmap DecodeColored(this SKCodec codec)
		{
			return SKBitmap.Decode(codec, new SKImageInfo(codec.Info.Width, codec.Info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
		}

		/// <summary>
		/// Resizes the current bitmap using the specified resize method.
		/// </summary>
		/// <param name="size">Desired image size.</param>
		/// <param name="method">The resize method.</param>
		public static SKBitmap Resize(this SKBitmap bitmap, Size size, SKFilterQuality quality = SKFilterQuality.High)
		{
			SKBitmap resized;
			var resizeInfo = new SKImageInfo(size.Width, size.Height);
			if (bitmap.ColorType != SKImageInfo.PlatformColorType)
			{
				// SkiaSharp sometimes does not support resizing non-platform color types
				using (var temp = bitmap.Copy(SKImageInfo.PlatformColorType))
					resized = temp.Resize(resizeInfo, quality);
			}
			else
				resized = bitmap.Resize(resizeInfo, quality);

			if (resized == null)
				throw new InvalidOperationException("Can not resize image");
			return resized;
		}

		/// <summary>
		/// Creates proportionally-resized image. Does not upscale image.
		/// Returns null if source bitmap size is the save as resized one.
		/// </summary>
		public static SKBitmap Resize(this SKBitmap bitmap, Size size, ResizeMode mode, SKFilterQuality quality = SKFilterQuality.High)
		{
			var sourceSize = new Size(bitmap.Width, bitmap.Height);
			var resultSize = size.GetResized(bitmap.Width, bitmap.Height, mode);
			if (sourceSize == resultSize)
				return null;

			if (mode == ResizeMode.Fill)
			{
				double scaleWidth = (double)resultSize.Width / sourceSize.Width;
				double scaleHeight = (double)resultSize.Height / sourceSize.Height;
				double scale = Math.Max(scaleWidth, scaleHeight);
				var fillSize = sourceSize.Multiply(scale);
				int left = (fillSize.Width - resultSize.Width) / 2;
				int top = (fillSize.Height - resultSize.Height) / 2;
				var rect = new SKRectI(left, top, left + resultSize.Width, top + resultSize.Height);

				var fillBitmap = new SKBitmap(resultSize.Width, resultSize.Height);
				using (var filled = bitmap.Resize(fillSize, quality))
					filled.ExtractSubset(fillBitmap, rect);
				return fillBitmap;
			}

			var fitSize = size.GetResized(bitmap.Width, bitmap.Height);
			var resized = bitmap.Resize(fitSize, quality);
			if (mode == ResizeMode.Fit)
				return resized;

			var padBitmap = new SKBitmap(resultSize.Width, resultSize.Height);
			using (var canvas = new SKCanvas(padBitmap))
			{
				int left = (resultSize.Width - fitSize.Width) / 2;
				int top = (resultSize.Height - fitSize.Height) / 2;
				canvas.Clear(SKColors.Transparent);
				canvas.DrawBitmap(resized, left, top);
			}
			resized.Dispose();
			return padBitmap;
		}

		/// <summary>
		/// Returns size of resized image. Saves proportions and does not upscale image.
		/// </summary>
		public static Size GetResized(this Size size, int imageWidth, int imageHeight, ResizeMode mode = ResizeMode.Fit)
		{
			if (imageWidth <= 0)
				throw new ArgumentException("Width must be positive", nameof(imageWidth));
			if (imageHeight <= 0)
				throw new ArgumentException("Height must be positive", nameof(imageHeight));

			if (size.IsEmpty)
				return new Size(imageWidth, imageHeight);
			else if (mode == ResizeMode.Pad)
				return new Size(size.Width == 0 ? imageWidth : size.Width, size.Height == 0 ? imageHeight : size.Height);

			int width = Math.Min(imageWidth, size.Width);
			int height = Math.Min(imageHeight, size.Height);
			if (width == 0)
				width = (int)Math.Round((double)imageWidth * height / imageHeight);
			else if (height == 0)
				height = (int)Math.Round((double)imageHeight * width / imageWidth);

			double scaleWidth = width / (double)imageWidth;
			double scaleHeight = height / (double)imageHeight;
			double scale = mode == ResizeMode.Fit ? Math.Min(scaleWidth, scaleHeight) : Math.Max(scaleWidth, scaleHeight);
			int resultWidth = (int)Math.Round(imageWidth * scale);
			int resultHeight = (int)Math.Round(imageHeight * scale);
			return new Size(Math.Min(resultWidth, width), Math.Min(resultHeight, height));
		}

		public static Size Multiply(this Size size, double scale)
		{
			return new Size((int)Math.Round(size.Width * scale), (int)Math.Round(size.Height * scale));
		}
	}
}