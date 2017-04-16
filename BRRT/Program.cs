﻿using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
namespace BRRT
{
	public static class Program
	{
		
		public static int Main(string[] args)
		{
			if (args.Length > 3) {
				//Parse arguments
				//1. Path 2. Start 3. Stop
				string MapPath = args [0];
				string start = args [1]; //x,y,phi
				start = start.Trim();
				string stop = args [2]; // x, y, phi
				stop = stop.Trim();

				string outputPath = args[3];
				string[] startArray = start.Split (new char[]{ ',' });
				string[] stopArray = stop.Split (new char[]{ ',' });
				Point StartPoint = new Point (Convert.ToInt32 (startArray [0]), Convert.ToInt32 (startArray [1]));
				double StartOrientation = Convert.ToDouble (startArray [2]);

				Point StopPoint = new Point (Convert.ToInt32 (stopArray [0]), Convert.ToInt32 (stopArray [1]));
				double StopOrientation = Convert.ToDouble (stopArray [2]);

				Console.WriteLine ("Start is: " + StartPoint.ToString () + " , " + StartOrientation);
				Console.WriteLine ("Stop is: " + StopPoint.ToString () + " , " + StopOrientation);
				string pathXml = "";
				if (args.Length > 4) {
					pathXml = args [4];
					Console.WriteLine ("Saving path to: " + pathXml);
				}

				if (File.Exists (MapPath)) {
					Bitmap BitMap = ImageHelpers.CreateNonIndexedImage( new Bitmap (MapPath));

					Map MyMap = new Map (BitMap);


					RRT Algorithm = new RRT(MyMap);
					Stopwatch watch = new Stopwatch();

					Algorithm.Finished += (object sender, EventArgs e) =>{
						watch.Stop();
						Console.WriteLine("Algorithm took: " + watch.ElapsedMilliseconds + " ms");
						//Event that gets called when the RRT is finished
						Console.WriteLine("Finished");

						List<RRTPath> paths = Algorithm.FindPathToTarget();



						if(paths.Count > 0)
						{


							RRTPath shortestPath = RRTPath.CleanPath(paths[0]);
							//Does not work yet
							//shortestPath.SaveToFile("Path.xml");
							//Syncing maps:
							//MyMap.SyncMapToImage();
							//MyMap.SaveBitmapToFile("Test.bmp");
							//RRTHelpers.DrawPath(paths[0],MyMap, Pens.AliceBlue);
							shortestPath.Color = Color.Red;
							RRTHelpers.DrawPath(shortestPath, MyMap, Pens.Cyan);


							//Ok optimize with the currently best path
							Console.WriteLine("Starting Optimization");
							PathOptimizer optimizer = new PathOptimizer(shortestPath, MyMap, Algorithm.EndRRTNode);
							watch.Reset();
							watch.Start();
							optimizer.Optimize();
							watch.Stop();
							Console.WriteLine("Optimizing took: " + watch.ElapsedMilliseconds + " ms");
							optimizer.Path.Color = Color.Blue;
							RRTHelpers.DrawPath(optimizer.Path, MyMap, Pens.DarkGoldenrod);
							if(pathXml != "")
							{
								optimizer.Path.SaveToFile(pathXml);
							}

						}
						else
						{
							Console.WriteLine("No paths found");

						}

						watch.Reset();
						watch.Start();
						MyMap.DrawLegend();
						//Draw the tree on the map
						//RRTHelpers.DrawTree(Algorithm.StartRRTNode, MyMap);

						RRTHelpers.DrawImportantNode(Algorithm.StartRRTNode, MyMap,5, Color.Red);
						//Draw the endpoint
						RRTHelpers.DrawImportantNode(Algorithm.EndRRTNode, MyMap,5, Color.Aqua);
						watch.Stop();
						Console.WriteLine("Drawing took: " + watch.ElapsedMilliseconds + " ms");
						if (paths.Count >0) 
						{
							//Save the result
							MyMap.SaveBitmapToFile(outputPath);
						}

						//Show the result in an Form (Can be used for debugging)
						/*Application.EnableVisualStyles();
						Application.SetCompatibleTextRenderingDefault(false);
						MainForm frm = new MainForm();
						frm.ShowImage(MyMap);

						frm.CreateTree(Algorithm.StartRRTNode);
						Application.Run(frm);
						*/




					};
					//Start the RRT
					watch.Start();
					Algorithm.Start (StartPoint, StartOrientation, StopPoint, StopOrientation);


					
				}
						
			}
			return 0;
		}
	}
}

