using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace StyleEl
{
	public class ImageOptions
	{
		Size _size;

		[Range(0, int.MaxValue)]
		public int Width
		{
			get { return _size.Width; }
			set { _size.Width = value; }
		}

		[Range(0, int.MaxValue)]
		public int Height
		{
			get { return _size.Height; }
			set { _size.Height = value; }
		}

		public ResizeMode Mode { get; set; }

		public Size Size { get => _size; set => _size = value; }

		public bool IsEmpty => _size.IsEmpty;
	}

	public enum ResizeMode
	{
		/// <summary>
		/// Resize to fit area.
		/// </summary>
		Fit,

		/// <summary>
		/// Resize to fill entire area centering image.
		/// </summary>
		Fill,

		/// <summary>
		/// Resize to fit area and add background padding.
		/// </summary>
		Pad
	}
}