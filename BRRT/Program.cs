using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
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
						//Event that gets called when the RRT is finished
						Console.WriteLine("Finished");
						//Draw the tree on the map
						RRTHelpers.DrawTree(Algorithm.StartRRTNode, MyMap);
						//Draw the endpoint
						RRTHelpers.DrawImportantNode(Algorithm.EndRRTNode, MyMap,5, Color.Aqua);
						//Save the result
						MyMap.SaveBitmapToFile("Result.bmp");
						//Show the result in an Form
						Application.EnableVisualStyles();
						Application.SetCompatibleTextRenderingDefault(false);
						MainForm frm = new MainForm();
						frm.ShowImage(MyMap.ImageMap);
						Application.Run(frm);



					};
					//Start the RRT
					Algorithm.Start (StartPoint, StartOrientation, StopPoint, StopOrientation);
					
				}
						
			}
			return 0;
		}
	}
}

