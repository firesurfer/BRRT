using System;
using System.Drawing;
using System.Drawing.Imaging;
namespace BRRT
{
	public static class ImageHelpers
	{
		public static Bitmap CreateNonIndexedImage(Image src)
		{
			Bitmap bmpIn = (Bitmap)src;

			Bitmap converted = new Bitmap(bmpIn.Width, bmpIn.Height, PixelFormat.Format24bppRgb);
			using (Graphics g = Graphics.FromImage(converted))
			{
				// Prevent DPI conversion
				g.PageUnit = GraphicsUnit.Pixel;
					// Draw the image
					g.DrawImageUnscaled(bmpIn, 0, 0);
			}
			return converted;
		}
	}
}

