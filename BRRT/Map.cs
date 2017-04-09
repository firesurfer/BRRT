using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing;
namespace BRRT
{
	public class Map
	{
		/// <summary>
		/// The map stored as int matrix. 255 -> is occupied, 0 is free
		/// </summary>
		/// <value>The matrix map.</value>
		public Matrix<double> MatrixMap;
		/// <summary>
		/// The map as an image.
		/// </summary>
		/// <value>The image map.</value>
		public Bitmap ImageMap { get;private set; }
		/// <summary>
		/// Gets the width of the map.
		/// </summary>
		/// <value>The width.</value>
		public int Width { get; private set; }
		/// <summary>
		/// Get the height of the map.
		/// </summary>
		/// <value>The height.</value>
		public int Height {get;private set;}

		/// <summary>
		/// Initializes a new instance of the <see cref="BRRT.Map"/> class.
		/// From a Bitmap.
		/// </summary>
		/// <param name="_Map">Map.</param>
		public Map (Bitmap _Map)
		{
			ImageMap = _Map;
			Width = _Map.Width;
			Height = _Map.Height;
			MatrixMap = Matrix<double>.Build.Dense (Width, Height);
			for (int x = 0; x < Width; x++) {
				for (int y = 0; y < Height; y++) {
					Color pixelColor = _Map.GetPixel (x, y);
					//If the pixel indicates an obstacle it's black -> Set value in matrix to 255
					//Else set the value to 0 -> It's free
					if (pixelColor.R > 200 && pixelColor.B > 200 && pixelColor.G > 200) {
						MatrixMap [x, y] = 255;
					} else {
						MatrixMap [x, y] = 0;
					}

				}
			}
			 
		}
		/// <summary>
		/// Get the matrix entry at x and y.
		/// (0,0) is at the center of the map -> We translate it into the internal map koordinates
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public int Get(int x, int y)
		{
			Point MapKoords = ToMapCoordinates (new Point (x, y));
			return (int)MatrixMap [MapKoords.X, MapKoords.X];
		}
		/// <summary>
		/// Determines whether this map point is occupied the specified x y.
		/// </summary>
		/// <returns><c>true</c> if this instance is occupied the specified x y; otherwise, <c>false</c>.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public bool IsOccupied(int x,int y)
		{
			
			Point MapKoords = ToMapCoordinates (new Point (x, y));
			if (MapKoords.X >= Width || MapKoords.Y >= Height)
				return true;
			if (MapKoords.X < 0 || MapKoords.Y < 0)
				return true;
			
			if (MatrixMap [MapKoords.X, MapKoords.Y] != 0)
				return true;
			return false;
		}
		/// <summary>
		/// Saves the bitmap to file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		public void SaveBitmapToFile(string FileName)
		{
			this.ImageMap.Save (FileName,System.Drawing.Imaging.ImageFormat.Bmp);
		}
		/// <summary>
		/// The map is starting in the upper left corner and is using positive values. We want to work in coordinates with {0,0} in the middle of the map.
		/// </summary>
		/// <returns>The map coordinates.</returns>
		/// <param name="_Point">Point.</param>
		public Point ToMapCoordinates(Point _Point)
		{
			return new Point (_Point.X + Width / 2,( _Point.Y*-1 +Height / 2));
		}
		/// <summary>
		/// From the map coordinates.
		/// </summary>
		/// <returns>The map coordinates.</returns>
		/// <param name="_Point">Point.</param>
		public Point FromMapCoordinates(Point _Point)
		{
			return new Point (_Point.X - Width / 2,( _Point.Y - Height / 2)*-1);
		}
		public void DrawPixelOnBitmap(Point _Point, Color col)
		{
			if (_Point.X < 0 || _Point.Y < 0)
				return;
			if (_Point.X >= Width || _Point.Y >= Height)
				return;

			ImageMap.SetPixel (_Point.X, _Point.Y, col);
		}


	}
}

