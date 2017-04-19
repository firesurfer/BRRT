using System;
using System.Drawing;
namespace BRRT
{
	public class PathOptimizer
	{
		/// <summary>
		/// Gets or sets the maximum drift angle.
		/// </summary>
		/// <value>The maximum drift angle.</value>
		public double MaximumDriftAngle{ get; set; }

		/// <summary>
		/// Gets or sets the minimum radius.
		/// </summary>
		/// <value>The minimum radius.</value>
		public double MinimumRadius{ get; set; }

		/// <summary>
		/// Gets or sets the amount of iterations.
		/// </summary>
		/// <value>The iterations.</value>
		public UInt32 Iterations { get; set; }

		/// <summary>
		/// Gets or sets the allowed orientation deviation.
		/// The angle we accept between the orientation of two points in order to accept them.
		/// </summary>
		/// <value>The allowed orientation deviation.</value>
		public double AllowedOrientationDeviation { get; set; }

		/// <summary>
		/// Gets the resulting path.
		/// </summary>
		/// <value>The path.</value>
		public RRTPath Path { get; private set; }

		/// <summary>
		/// Gets the step width straight.
		/// </summary>
		/// <value>The step width straight.</value>
		public int StepWidthStraight { get; private set; }

		/// <summary>
		/// Gets the internal map the optimizer runs on.
		/// </summary>
		/// <value>The internal map.</value>
		public Map InternalMap { get; private set; }

		/// <summary>
		/// Gets the end point. The point we want to navigate to
		/// </summary>
		/// <value>The end point.</value>
		public RRTNode EndPoint { get; private set; }

		/// <summary>
		/// Gets or sets the step width we use for optimizing our path towards the EndPoint.
		/// </summary>
		/// <value>The step width end.</value>
		public double StepWidthEnd { get; set; }

		/// <summary>
		/// Gets or sets the progress. (Used for printing the progress)
		/// </summary>
		/// <value>The progress.</value>
		private double Progress { get; set; }



		public double StepWidthCurve{ get; set;}
		/// <summary>
		/// Initializes a new instance of the <see cref="BRRT.PathOptimizer"/> class.
		/// </summary>
		/// <param name="_Path">Path.</param>
		/// <param name="_Map">Map.</param>
		/// <param name="_EndPoint">End point.</param>
		public PathOptimizer (RRTPath _Path, Map _Map, RRTNode _EndPoint)
		{
			this.InternalMap = _Map;
			this.Path = _Path;
			this.Iterations = 10;
			this.MaximumDriftAngle = 10;
			this.MinimumRadius = 20;
			this.AllowedOrientationDeviation = 1;

			this.StepWidthStraight = 7;
			this.EndPoint = _EndPoint;
			this.StepWidthEnd = 4;
			this.StepWidthCurve = 5;
		}

		/// <summary>
		/// Start the path optimization.
		/// </summary>
		public void Optimize ()
		{
			Console.WriteLine ("Optimizing:");
			//OptimizeForEndPoint ();
			//OptimizeStraight ();
			//OptimizeForEndPoint ();

			//
			OptimizeCurves ();
		}

		/// <summary>
		/// Optimize our path so we hit the endpoint
		/// </summary>
		public void OptimizeForEndPoint ()
		{
			//Go along from then nearest point to the endpoint
			RRTNode previous = Path.Start;
			Console.WriteLine ("Path length before optimization for endpoint: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost ());
			Console.WriteLine ();

			while (previous != null) {

				if (previous == null)
					break;
				//Check if the orientation of the selected point is nearly the same as the orientation of the endpoint
				if (Math.Abs (previous.Orientation - EndPoint.Orientation) < AllowedOrientationDeviation * 5) {
					//Okey connect them
					RRTNode selectedNode = previous;
					RRTNode lastNode = null;
					//Create a clone we can work on
					RRTNode start = selectedNode.Clone ();
					double Distance = RRTHelpers.CalculateDistance (selectedNode, EndPoint);
					double angle = RRTHelpers.CalculateAngle (selectedNode, EndPoint);
					if (Math.Abs(RRTHelpers.SanatizeAngle (angle * RRTHelpers.ToDegree)) > this.MaximumDriftAngle) {
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
			Console.WriteLine ("Path length after optimization for endpoint: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost ());

		}

		/// <summary>
		/// Optimize or path by taking to points and check if we can connect them straight.
		/// </summary>
		public void OptimizeStraight ()
		{
			double PreviousProgress = 0;

			Console.WriteLine ("Path length before optimization: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost ());
			Random random = new Random (System.DateTime.Now.Millisecond);
			for (UInt32 it = 0; it < Iterations; it++) {

				Progress = (int)(Math.Round (((double)it / (double)Iterations) * 100));
				if (Progress != PreviousProgress) {
					PreviousProgress = Progress;
					PrintProgress ();
				}
				//Select two random points
				int indexNode1 = random.Next (50, Path.CountNodes - 1);
				
				RRTNode node1 = Path.SelectNode (indexNode1);
				RRTNode node2 = Path.SelectNode (random.Next (1, indexNode1));

				//Check if they have roughly the same orientation
				if (Math.Abs (node1.Orientation - node2.Orientation) < AllowedOrientationDeviation) {
					//Calculate distance between points
					double Distance = RRTHelpers.CalculateDistance (node1, node2);
					if (Distance < 10)
						continue;
					//Calculate angle between points
					double angle = RRTHelpers.CalculateAngle (node1, node2);
					//Console.WriteLine ("Selected: " + node1 + " " + node2 + " Distance: " + Distance + " Angle: " + angle);

					if (Math.Abs (angle * RRTHelpers.ToDegree) > this.MaximumDriftAngle)
						continue;
					if (node1.Inverted != node2.Inverted)
						continue;
					RRTNode start = node1.Clone (); //new RRTNode(node1.Position,node1.Orientation, null);
					RRTNode end = node2.Clone (); //new RRTNode (node2.Position, node2.Orientation, null);

					RRTNode lastNode = null;
					bool success = true;

					//Connect them
					for (double i = 0; i <= Distance; i += StepWidthStraight) {
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
						} else
							Console.WriteLine ("Node2.Successor[0] was null");
						node1.Successors.Clear ();
						node2.Successors.Clear ();
						node2.Predecessor = null;
						node1.Predecessor = null;
						Path.CalculateLength ();
					
					}
				}


				
			}
			Path.CalculateLength ();
			Console.WriteLine ("Path length after opt: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost ());
		}

		/// <summary>
		/// Optimize two points taken from the path by trying to connect them via a curve.
		/// </summary>
		public void OptimizeCurves ()
		{
			Console.WriteLine ("Path length before optimization: " + Path.Length + " Count: " + Path.CountNodes + " Cost: " + Path.Cost ());
			Random random = new Random (System.DateTime.Now.Millisecond);
			for (UInt32 it = 0; it < Iterations; it++) {
				RRTNode start;
				RRTNode end;
				//Select two random points
				SelectPoints (random, out start,out  end);
				//Calculate distance
				double distance = RRTHelpers.CalculateDistance(start,end);
				//Distance is too small that it makes sense to optimize it
				if (distance < 10)
					continue;
				//Calculate angle delta (angle between orientations) (in degrees)
				double delta = end.Orientation - start.Orientation;
				//Calculate angle between points (in radians)
				double angle = RRTHelpers.CalculateAngle(start,end);

				//We can't go from inverted to not inverted (not from forward to backwards or the other way round)
				if (start.Inverted != end.Inverted)
					continue;

				//Now decide if going straight is way to go
				//NOTE delta ist entweder sehr klein oder sehr groß (fast 360°, siehe Hilfsfunktion "anglesAreClose" in pseudocode)
				if (Math.Abs (delta) < AllowedOrientationDeviation && Math.Abs(angle*RRTHelpers.ToRadians) < MaximumDriftAngle) {
					//The deviation in the orientation is small enough we can accept going straight (or drift)
					//And the angle between the points is smaller than the maximum drift we can do


					//Step straight or in a drift. This function may manipulate the path
					//StepStraight (start, end, distance, angle);
				} else {
					//We try a curve


					double theta = RRTHelpers.SanatizeAngle (angle*RRTHelpers.ToDegree + Math.Sign (delta) * (180 - Math.Abs (delta)) / 2)*RRTHelpers.ToRadians;
					double radius = Math.Abs (distance / (2 * Math.Sin (delta*RRTHelpers.ToRadians / 2)));
					//Check if the radius is > minimum radius
					if (radius < MinimumRadius)
						continue;

					//Calculate middle point
					double midX = start.Position.X + Math.Cos(theta) * radius;
					double midY = start.Position.Y + Math.Sin(theta) * radius;

					RRTHelpers.DrawImportantNode (new RRTNode (new System.Drawing.Point ((int)midX, (int)midY), theta, null), InternalMap, 5, System.Drawing.Color.DarkMagenta);
					double gamma = start.Orientation - RRTHelpers.SanatizeAngle (theta*RRTHelpers.ToDegree - Math.Sign (delta) * 90);

					double driftAngle = gamma; //In degree
					double curveRadius = delta / radius; //In degree per meter

                    //NOTE Hier stimmte noch was nicht. MinimumRadius/curveRadius hatte noch Einheiten => statt curveRadius nur radius verwenden. Würde man die tatsächlichen Krümmungen einsetzen, würde sich das delta sowieso rauskürzen
                    if (driftAngle*RRTHelpers.ToRadians / MaximumDriftAngle + MinimumRadius/ radius >= 1)
						continue;


					Console.WriteLine ("Stepping to curve");
					StepCurve (start, end, delta, new System.Drawing.Point ((int)midX, (int)midY), radius, angle, theta);
				}
			}

		}
		private void StepCurve(RRTNode node1, RRTNode node2, double delta, System.Drawing.Point middle, double radius, double angle, double theta)
		{
			double outerlength = Math.Abs (delta * RRTHelpers.ToRadians * radius);
			double steps = outerlength / StepWidthCurve;
			double angleStep = delta / steps;

			RRTNode start = node1.Clone (); 
			RRTNode end = node2.Clone (); 

			RRTNode lastNode = null;
			bool success = true;

			for (int i = 0; i < (int)steps; i++) {
				int NewX = (int)(middle.X - Math.Cos (RRTHelpers.SanatizeAngle (theta + i * angleStep)*RRTHelpers.ToRadians) * radius);
				int NewY = (int)(middle.Y - Math.Sin (RRTHelpers.SanatizeAngle (theta + i * angleStep)*RRTHelpers.ToRadians) * radius);
				double NewOrientation = RRTHelpers.SanatizeAngle (node1.Orientation + i * angleStep);
				if (InternalMap.IsOccupied (NewX, NewY)) {
					success = false;
					break;
				}

				RRTNode newNode = null;
				if (lastNode == null) {
					newNode = new RRTNode (new System.Drawing.Point (NewX, NewY),NewOrientation, start);
					newNode.Inverted = start.Inverted;
					start.Successors.Add (newNode);
				} else {
					newNode = new RRTNode (new System.Drawing.Point (NewX, NewY), NewOrientation, lastNode);
					lastNode.Successors.Add (newNode);
					newNode.Inverted = lastNode.Inverted;
					//RRTHelpers.DrawImportantNode (newNode, InternalMap, 5, Color.DarkOrange);
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
				} else
					Console.WriteLine ("Node2.Successor[0] was null");
				node1.Successors.Clear ();
				node2.Successors.Clear ();
				node2.Predecessor = null;
				node1.Predecessor = null;
				Path.CalculateLength ();

			}


		}
		private void StepStraight(RRTNode node1, RRTNode node2, double Distance, double Angle)
		{
			RRTNode start = node1.Clone (); 
			RRTNode end = node2.Clone (); 

			RRTNode lastNode = null;
			bool success = true;

			//Connect them
			for (double i = 0; i <= Distance; i += StepWidthStraight) {
				int NewX = (int)(start.Position.X + i * Math.Cos (Angle));
				int NewY = (int)(start.Position.Y + i * Math.Sin (Angle));

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
				} else
					Console.WriteLine ("Node2.Successor[0] was null");
				node1.Successors.Clear ();
				node2.Successors.Clear ();
				node2.Predecessor = null;
				node1.Predecessor = null;
				Path.CalculateLength ();

			}
		
		}

		private void SelectPoints(Random random, out RRTNode node1, out RRTNode node2)
		{
			//Select two random points
			int indexNode1 = random.Next (50, Path.CountNodes - 1);

			node1 = Path.SelectNode (indexNode1);
			node2 = Path.SelectNode (random.Next (1, indexNode1));

		}

		/// <summary>
		/// Prints the progress.
		/// </summary>
		private void PrintProgress ()
		{
			Console.SetCursorPosition (0, Console.CursorTop - 1);

			Console.WriteLine ("Progress optimizing: " + Progress + "%");

		}
	}
}

