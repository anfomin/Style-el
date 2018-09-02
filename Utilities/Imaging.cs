using System;
using System.Drawing;
using System.IO;
using SkiaSharp;

namespace StyleEl
{
	/// <summary>
	/// Provides methods for image processing.
	/// </summary>
	public static class Imaging
	{
		public static SKCodec CreateCodec(Stream stream)
		{
			return SKCodec.Create(WrapManagedStream(stream));
		}

		/// <summary>
		/// Decodes image from codec with RGBA8888 or BGRA8888 color type.
		/// </summary>
		public static SKBitmap DecodeColored(Stream stream)
		{
			using (var codec = CreateCodec(stream))
				return codec.DecodeColored();
		}

		/// <summary>
		/// Rotates image according EXIF orientation.
		/// </summary>
		/// <returns>Rotates bitmap or source bitmap if rotation does not required.</returns>
		public static SKBitmap HandleOrientation(this SKBitmap bitmap, SKCodecOrigin orientation)
		{
			SKBitmap result;
			switch (orientation)
			{
				case SKCodecOrigin.BottomRight: // rotated 180
					result = new SKBitmap(bitmap.Width, bitmap.Height);
					using (var surface = new SKCanvas(result))
					{
						surface.RotateDegrees(180, bitmap.Width / 2, bitmap.Height / 2);
						surface.DrawBitmap(bitmap, 0, 0);
					}
					return result;
				case SKCodecOrigin.RightTop: // rotated 90 cw
					result = new SKBitmap(bitmap.Height, bitmap.Width);
					using (var surface = new SKCanvas(result))
					{
						surface.Translate(result.Width, 0);
						surface.RotateDegrees(90);
						surface.DrawBitmap(bitmap, 0, 0);
					}
					return result;
				case SKCodecOrigin.LeftBottom: // rotated 90 ccw
					result = new SKBitmap(bitmap.Height, bitmap.Width);
					using (var surface = new SKCanvas(result))
					{
						surface.Translate(0, result.Height);
						surface.RotateDegrees(270);
						surface.DrawBitmap(bitmap, 0, 0);
					}
					return result;
				default:
					return null;
			}
		}

		public static Stream Resize(Stream stream, ImageOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (options.IsEmpty)
				return null;

			Stream result;
			SKBitmap bitmap = null;
			try
			{
				using (var codec = Imaging.CreateCodec(stream))
				{
					bitmap = codec.DecodeColored();

					// handle EXIF orientation
					if (codec.Origin != SKCodecOrigin.TopLeft &&
						bitmap.HandleOrientation(codec.Origin) is SKBitmap rotated)
					{
						bitmap.Dispose();
						bitmap = rotated;
					}

					// handle resize
					if (bitmap.Resize(options.Size, options.Mode) is SKBitmap resized)
					{
						bitmap.Dispose();
						bitmap = resized;
					}

					result = bitmap.EncodeDefaultWhite(codec.EncodedFormat).AsStream(true);
				}
			}
			catch (Exception ex)
			{
				throw new FormatException("Image format is not supported", ex);
			}
			finally
			{
				if (bitmap != null)
					bitmap.Dispose();
			}
			return result;
		}

		static SKStream WrapManagedStream(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			if (stream.CanSeek)
				return new SKManagedStream(stream);
			else
				return new SKFrontBufferedManagedStream(stream, SKCodec.MinBufferedBytesNeeded);
		}
	}
}