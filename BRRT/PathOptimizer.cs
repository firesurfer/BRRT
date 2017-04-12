using System;

namespace BRRT
{
	public class PathOptimizer
	{
		public double MaximumDriftAngle{get;set;}
		public double MinimumRadius{get;set;}
		public UInt32 Iterations { get; set;}
		public double AllowedOrientationDeviation {get;set;}
		public double SearchDistance{ get; set;}
		public RRTPath Path {get;private set;}
		public int StepWidthStraight { get; private set;}
		public Map InternalMap { get; private set;}
		public double MinimumDistance;
		public PathOptimizer (RRTPath _Path, Map _Map)
		{
			this.InternalMap = _Map;

			this.Path = _Path;
			this.Iterations = 5000;
			this.MaximumDriftAngle = 20;
			this.MinimumRadius = 20;
			this.AllowedOrientationDeviation = 10;
			this.SearchDistance = 700;
			this.MinimumDistance = 100;
			this.StepWidthStraight = 2;
		}
		public void Optimize()
		{
			OptimizeStraight ();
			OptimizeCurves ();
		}
		public void OptimizeStraight()
		{
			Console.WriteLine ("Path length before optimization: " + Path.Length + " Count: " + Path.CountNodes);
			Random random = new Random (System.DateTime.Now.Second);
			for (UInt32 it = 0; it < Iterations; it++) {
				//Select two random points
				int indexNode1 = random.Next(Path.CountNodes-1);
				RRTNode node1 = Path.SelectNode(indexNode1);
				RRTNode node2 = Path.SelectNode (random.Next(indexNode1));

				//Check if they have roughly the same orientation
				if (Math.Abs(node1.Orientation - node2.Orientation )< AllowedOrientationDeviation) {
					//Calculate distance between points
					double Distance = RRTHelpers.CalculateDistance (node1, node2);
					if (Distance < 10)
						continue;
					//Calculate angle between points
					double angle = RRTHelpers.CalculateAngle (node1, node2);
					//Console.WriteLine ("Selected: " + node1 + " " + node2 + " Distance: " + Distance + " Angle: " + angle);


					RRTNode start = new RRTNode(node1.Position,node1.Orientation, null);
					RRTNode end = new RRTNode (node2.Position, node2.Orientation, null);
					//check if start is predecessor of end else swap them

					/*RRTNode temp = end;
					bool isPredecessor = false;
					while (temp != null) {
						if (temp.Position == start.Position) {
							isPredecessor = true;
						}
						temp = temp.Predecessor;
					}
					if (isPredecessor) {
						temp = start;
						start = end;
						end = start;
					}*/

					RRTNode lastNode = null;
					bool success = true;

					//Connect them
					for (double i = 0; i <= Distance; i+= StepWidthStraight) {
						int NewX = (int)(start.Position.X + i * Math.Cos (angle));
						int NewY = (int)(start.Position.Y + i * Math.Sin (angle));
						Console.WriteLine ("Oc: " + InternalMap.IsOccupied (NewX, NewY) + " X: " + NewX + " Y: " + NewY + "  " + start);
						if (InternalMap.IsOccupied (NewX, NewY)) {
							success = false;
							break;
						}

						RRTNode newNode = null;
						if (lastNode == null) {
							newNode = new RRTNode (new System.Drawing.Point (NewX, NewY), node1.Orientation, start);
							start.Successors.Add (newNode);
						} else {
							newNode = new RRTNode (new System.Drawing.Point (NewX, NewY), node1.Orientation, lastNode);
							lastNode.Successors.Add (newNode);
						}
						lastNode = newNode;
					}
					if (lastNode == null)
						success = false;

					//We successfully connected them
					if (success) {
						end.Predecessor = lastNode;
						lastNode.AddSucessor (end);
						if (node1.Predecessor != null) {
							node1.Predecessor.Successors.Clear ();
							node1.Predecessor.AddSucessor (start);

							start.Predecessor = node1.Predecessor;
						}
						node1.Predecessor = null;
						if (node2.Successors.Count > 0) {
							end.AddSucessor (node2.Successors [0]);
							node2.Successors [0].Predecessor = end;
							node2.Predecessor = null;
							node2.Successors.Clear ();
						}
						Path.CalculateLenght ();
						Console.WriteLine ("It: " + it + " Count: " + Path.CountNodes);

						//Console.WriteLine ("Path length: " + Path.Length + " Count: " + Path.CountNodes);
					}
				}

				
			}
			//Path = new RRTPath (Path.Start, Path.End);
			Path.CalculateLenght();
			Console.WriteLine ("Path length after opt: " + Path.Length + " Count: " + Path.CountNodes);

		}
		public void OptimizeCurves()
		{

		}
		private void ClearChildsTillNode(RRTNode baseNode, RRTNode endNode)
		{

		}
	}
}

