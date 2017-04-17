using System;

namespace BRRT
{
	public class PathOptimizer
	{
		public double MaximumDriftAngle{get;set;}
		public double MinimumRadius{get;set;}
		public UInt32 Iterations { get; set;}
		public double AllowedOrientationDeviation {get;set;}

		public RRTPath Path {get;private set;}
		public int StepWidthStraight { get; private set;}
		public Map InternalMap { get; private set;}
		public double MinimumDistance;
		public RRTNode EndPoint { get; private set;}
		public double StepWidthEnd { get; private set;}

		private double Progress {get;  set;}
		public PathOptimizer (RRTPath _Path, Map _Map,RRTNode _EndPoint)
		{
			this.InternalMap = _Map;

			this.Path = _Path;
			this.Iterations = 500000;
			this.MaximumDriftAngle = 10;
			this.MinimumRadius = 20;
			this.AllowedOrientationDeviation = 3;
			this.MinimumDistance = 100;
			this.StepWidthStraight = 7;
			this.EndPoint = _EndPoint;
			this.StepWidthEnd = 4;

		}
		public void Optimize()
		{
			
			OptimizeForEndPoint ();
			OptimizeStraight ();
			OptimizeForEndPoint ();

			//
			//OptimizeCurves ();
		}
		public void OptimizeForEndPoint()
		{
			//Go along from then nearest point to the endpoint
			RRTNode previous = Path.Start;
			Console.WriteLine ("Path length before optimization for endpoint: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost());
			Console.WriteLine ();

			while (previous != null) {

				if (previous == null)
					break;
				//Check if the orientation of the selected point is nearly the same as the orientation of the endpoint
				if (Math.Abs(previous.Orientation - EndPoint.Orientation) < AllowedOrientationDeviation*5) {
					//Okey connect them
					RRTNode selectedNode = previous;
					RRTNode lastNode = null;
					//Create a clone we can work on
					RRTNode start = selectedNode.Clone ();
					double Distance = RRTHelpers.CalculateDistance (selectedNode, EndPoint);
					double angle = RRTHelpers.CalculateAngle (selectedNode, EndPoint);
					if (RRTHelpers.SanatizeAngle(angle * RRTHelpers.ToDegree) > this.MaximumDriftAngle) {
						previous = previous.Predecessor;
						continue;
					}
					bool success = true;

					//Connect them
					for (double i = 0; i <= Distance; i += StepWidthEnd) {
						//Create new point
						int NewX = (int)(selectedNode.Position.X + i * Math.Cos (angle));
						int NewY = (int)(selectedNode.Position.Y + i * Math.Sin (angle));

						//Check if this point is occupied
						if (InternalMap.IsOccupied (NewX, NewY)) {
							success = false;
							break;
						}


						RRTNode newNode = null;
						if (lastNode == null) {
							newNode = new RRTNode (new System.Drawing.Point (NewX, NewY), start.Orientation, start);
							start.Successors.Add (newNode);
						} else {
							newNode = new RRTNode (new System.Drawing.Point (NewX, NewY), start.Orientation, lastNode);
							lastNode.Successors.Add (newNode);
						}
						lastNode = newNode;

					}
					if (lastNode == null)
						success = false;
					if (success) {
						Path.Start = lastNode;
						//Replace the selectNode with our start node. 
						start.Predecessor = selectedNode.Predecessor;
						selectedNode.Predecessor.Successors.Clear ();
						selectedNode.Predecessor.AddSucessor (start);
						selectedNode.Predecessor = null;
						selectedNode.Successors.Clear ();
						previous = start;
					}
				}
				previous = previous.Predecessor;

			}
			Path.CalculateLength ();
			Console.WriteLine ("Path length after optimization for endpoint: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost());

		}
		public void OptimizeStraight()
		{
			double PreviousProgress = 0;

			Console.WriteLine ("Path length before optimization: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost());
			Random random = new Random (System.DateTime.Now.Millisecond);
			for (UInt32 it = 0; it < Iterations; it++) {

				Progress = (int)(Math.Round(((double)it / (double)Iterations)*100));
				if (Progress != PreviousProgress) {
					PreviousProgress = Progress;
					PrintProgress ();
				}
				//Select two random points
				int indexNode1 = random.Next(1,Path.CountNodes-1);
				RRTNode node1 = Path.SelectNode(indexNode1);
				RRTNode node2 = Path.SelectNode (random.Next(1,indexNode1));

				//Check if they have roughly the same orientation
				if (Math.Abs(node1.Orientation - node2.Orientation )< AllowedOrientationDeviation) {
					//Calculate distance between points
					double Distance = RRTHelpers.CalculateDistance (node1, node2);
					if (Distance < 10)
						continue;
					//Calculate angle between points
					double angle = RRTHelpers.CalculateAngle (node1, node2);
					//Console.WriteLine ("Selected: " + node1 + " " + node2 + " Distance: " + Distance + " Angle: " + angle);

					if (RRTHelpers.SanatizeAngle(angle * RRTHelpers.ToDegree) > this.MaximumDriftAngle)
						continue;
					if (node1.Inverted != node2.Inverted)
						continue;
					RRTNode start = node1.Clone (); //new RRTNode(node1.Position,node1.Orientation, null);
					RRTNode end = node2.Clone(); //new RRTNode (node2.Position, node2.Orientation, null);

					RRTNode lastNode = null;
					bool success = true;

					//Connect them
					for (double i = 0; i <= Distance; i+= StepWidthStraight) {
						int NewX = (int)(start.Position.X + i * Math.Cos (angle));
						int NewY = (int)(start.Position.Y + i * Math.Sin (angle));

						if (InternalMap.IsOccupied (NewX, NewY)) {
							success = false;
							break;
						}

						RRTNode newNode = null;
						if (lastNode == null) {
							newNode = new RRTNode (new System.Drawing.Point (NewX, NewY), node1.Orientation, start);
							newNode.Inverted = start.Inverted;
							start.Successors.Add (newNode);
						} else {
							newNode = new RRTNode (new System.Drawing.Point (NewX, NewY), node1.Orientation, lastNode);
							lastNode.Successors.Add (newNode);
							newNode.Inverted = lastNode.Inverted;
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
						} else
							Console.WriteLine ("Node1.Predecessor was null");

						if (node2.Successors.Count > 0) {
							end.AddSucessor (node2.Successors [0]);
							node2.Successors [0].Predecessor = end;
							node2.Predecessor = null;
							node2.Successors.Clear ();
						}
						else
							Console.WriteLine ("Node2.Successor[0] was null");
						node1.Successors.Clear ();
						node2.Successors.Clear ();
						node2.Predecessor = null;
						node1.Predecessor = null;
						Path.CalculateLength ();
					
					}
				}

				
			}
			Path.CalculateLength();
			Console.WriteLine ("Path length after opt: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost());
		}
		// Testimplementierung von Kurven&Drift Optimierung
		public void OptimizeCurves()
		{
			//dh. abs(kurve/kurvemax) + abs(drift/driftmax) <=1 sein

			double PreviousProgress = 0;

			Console.WriteLine ("Path length before optimization: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost());
			Random random = new Random (System.DateTime.Now.Second);
			for (UInt32 it = 0; it < Iterations; it++) {

				Progress = (int)(Math.Round(((double)it / (double)Iterations)*100));
				if (Progress != PreviousProgress) {
					PreviousProgress = Progress;
					PrintProgress ();
				}
				//Select two random points
				int indexNode1 = random.Next(1,Path.CountNodes-1);
				RRTNode node1 = Path.SelectNode(indexNode1);
				RRTNode node2 = Path.SelectNode (random.Next(1,indexNode1));

				//Check that they have NOT the same orientation
				if (Math.Abs(node1.Orientation - node2.Orientation ) > AllowedOrientationDeviation) {
					//Calculate distance between points
					double Distance = RRTHelpers.CalculateDistance (node1, node2);
					// TODO mindistance
					if (Distance < MinimumRadius*2)
						continue;
					//Calculate angle between points
					double angle = RRTHelpers.CalculateAngle (node1, node2);
					//Console.WriteLine ("Selected: " + node1 + " " + node2 + " Distance: " + Distance + " Angle: " + angle);

					if (RRTHelpers.SanatizeAngle(angle * RRTHelpers.ToDegree) > this.MaximumDriftAngle)
						continue;
					if (node1.Inverted != node2.Inverted)
						continue;
					RRTNode start = new RRTNode(node1.Position,node1.Orientation, null);
					RRTNode end = new RRTNode (node2.Position, node2.Orientation, null);


					RRTNode lastNode = null;
					bool success = true;

					//Connect them
					for (double i = 0; i <= Distance; i+= StepWidthStraight) {
						int NewX = (int)(start.Position.X + i * Math.Cos (angle));
						int NewY = (int)(start.Position.Y + i * Math.Sin (angle));

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
						} else
							Console.WriteLine ("Node1.Predecessor was null");

						if (node2.Successors.Count > 0) {
							end.AddSucessor (node2.Successors [0]);
							node2.Successors [0].Predecessor = end;
							node2.Predecessor = null;
							node2.Successors.Clear ();
						}
						else
							Console.WriteLine ("Node2.Successor[0] was null");
						node1.Successors.Clear ();
						node2.Successors.Clear ();
						node2.Predecessor = null;
						node1.Predecessor = null;
						Path.CalculateLength ();

					}
				}


			}	
			Path.CalculateLength();
			Console.WriteLine ();
			Console.WriteLine ("Path length after opt: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost());


		}
		private void ClearChildsTillNode(RRTNode baseNode, RRTNode endNode)
		{

		}
		private void PrintProgress()
		{
			Console.SetCursorPosition (0, Console.CursorTop-1);

			Console.WriteLine ("Progress optimizing: " + Progress + "%");

		}
	}
}

