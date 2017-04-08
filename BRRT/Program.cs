using System;
using System.IO;
using System.Drawing;
namespace BRRT
{
	public static class Program
	{
		
		public static int Main(string[] args)
		{
			if (args.Length > 2) {
				//Parse arguments
				//1. Path 2. Start 3. Stop
				string MapPath = args [0];
				string start = args [1]; //x,y,phi
				start = start.Trim();
				string stop = args [2]; // x, y, phi
				stop = stop.Trim();

				string[] startArray = start.Split (new char[]{ ',' });
				string[] stopArray = stop.Split (new char[]{ ',' });
				Point StartPoint = new Point (Convert.ToInt32 (startArray [0]), Convert.ToInt32 (startArray [1]));
				double StartOrientation = Convert.ToDouble (startArray [2]);

				Point StopPoint = new Point (Convert.ToInt32 (stopArray [0]), Convert.ToInt32 (stopArray [1]));
				double StopOrientation = Convert.ToDouble (stopArray [2]);

				Console.WriteLine ("Start is: " + StartPoint.ToString () + " , " + StartOrientation);
				Console.WriteLine ("Stop is: " + StopPoint.ToString () + " , " + StopOrientation);

				if (File.Exists (MapPath)) {
					Bitmap BitMap = ImageHelpers.CreateNonIndexedImage( new Bitmap (MapPath));

					Map MyMap = new Map (BitMap);
					RRT Algorithm = new RRT(MyMap);
					Algorithm.Finished += (object sender, EventArgs e) =>{
						Console.WriteLine("Finished");
						RRTHelpers.DrawTree(Algorithm.StartRRTNode, MyMap);
						RRTHelpers.DrawImportantNode(Algorithm.EndRRTNode, MyMap,5, Color.Aqua);
						MyMap.SaveBitmapToFile("Result.bmp");
					};
					Algorithm.Start (StartPoint, StartOrientation, StopPoint, StopOrientation);
					
				}
						
			}
			return 0;
		}
	}
}

