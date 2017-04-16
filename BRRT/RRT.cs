using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet;

namespace BRRT
{
	public class RRT
	{
		/// <summary>
		/// The map we are working on.
		/// </summary>
		public Map InternalMap { get; private set; }
		/// <summary>
		/// Start Point of the RRT.
		/// </summary>
		/// <value>The start point.</value>
		public Point StartPoint { get; private set; }
		/// <summary>
		/// The Robot orientation at the start point.
		/// </summary>
		/// <value>The start orientation.</value>
		public double StartOrientation { get; private set; }
		/// <summary>
		/// The start RRT node.
		/// </summary>
		public RRTNode StartRRTNode { get; private set; }
		/// <summary>
		/// EndPoint of the RRT
		/// </summary>
		/// <value>The end point.</value>
		public Point EndPoint { get; private set; }
		/// <summary>
		/// The orientation of the robot at the endpoint
		/// </summary>
		/// <value>The end orientation.</value>
		public double EndOrientation { get; private set; }
		/// <summary>
		/// The EndPoint and Orientation summarized in an RRTNode
		/// </summary>
		public RRTNode EndRRTNode { get; private set; }
		/// <summary>
		/// Gets or sets the amount of iterations.
		/// </summary>
		/// <value>The iterations.</value>
		public UInt32 Iterations { get; set; }
		/// <summary>
		/// Occurs when algorithm has finished.
		/// </summary>
		public event EventHandler<EventArgs> Finished;

		/// <summary>
		/// List of All valid nodes so we don't need to iterate over the tree spanned from the StartPoint.
		/// </summary>
		private List<RRTNode> AllNodes = new List<RRTNode>();

		/// <summary>
		/// Gets or sets the minumum radius.
		/// </summary>
		/// <value>The minumum radius.</value>
		public double MinumumRadius { get; set; }

		/// <summary>
		/// Gets or sets the maximum drift.
		/// </summary>
		/// <value>The maximum drift.</value>
		public double MaximumDrift { get; set; }

		/// <summary>
		/// The width of one incremental step when stepping towards new node
		/// </summary>
		/// <value>The width of the step.</value>
		public int StepWidth { get; set; }

		/// <summary>
		/// Gets or sets the width of the circle step.
		/// </summary>
		/// <value>The width of the circle step.</value>
		public int CircleStepWidth { get; set; }
		/// <summary>
		/// Gets or sets the target area. Search area around target.
		/// </summary>
		/// <value>The target area.</value>
		public Rectangle TargetArea { get; set; }

		/// <summary>
		/// Gets or sets the acceptable orientation deviation from the target orientation.
		/// </summary>
		/// <value>The acceptable orientation deviation.</value>
		public double AcceptableOrientationDeviation { get; set; }

		/// <summary>
		/// Value between 0 and 256. A smaller value will prefer curves. A higher value will prefer straight lines.
		/// </summary>
		/// <value>The prefer straight.</value>
		public int PreferStraight { get; set; }

		/// <summary>
		/// Value between 0 and 256. Smaller value will prefer forward. Higher value will prefer 
		/// </summary>
		/// <value>The straight invert probability.</value>
		public int StraightInvertProbability { get; set; }

		/// <summary>
		/// Gets the progress.
		/// </summary>
		/// <value>The progress.</value>
		public int Progress { get; private set; }
		/// <summary>
		/// Initializes a new instance of the <see cref="BRRT.RRT"/> class.
		/// </summary>
		/// <param name="_Map">Map.</param>
		public RRT(Map _Map)
		{
			this.InternalMap = _Map;
			this.Iterations = 150000;
			this.MaximumDrift = 20;
			this.StepWidth = 5;
			this.CircleStepWidth = 10;
			this.MinumumRadius = 20;
			this.TargetArea = new Rectangle(0, 0,50, 50);
			this.AcceptableOrientationDeviation = 4;
			this.PreferStraight = 175;
			this.StraightInvertProbability = 125;
		}

		/// <summary>
		/// Start the RRT.
		/// </summary>
		/// <param name="_Start">Start.</param>
		/// <param name="_StartOrientation">Start orientation.</param>
		/// <param name="_End">End.</param>
		/// <param name="_EndOrientation">End orientation.</param>
		public void Start(Point _Start, double _StartOrientation, Point _End, double _EndOrientation)
		{
			this.StartPoint = InternalMap.FromMapCoordinates(_Start);
			if (PointValid(StartPoint))
				throw new Exception("StartPoint in invalid region");
			this.StartOrientation = _StartOrientation;
			this.StartRRTNode = new RRTNode(StartPoint, StartOrientation, null);
			this.EndPoint = InternalMap.FromMapCoordinates(_End);
			if (PointValid(EndPoint))
				throw new Exception("EndPoint in invalid region");
			this.EndOrientation = _EndOrientation;
			this.EndRRTNode = new RRTNode(EndPoint, EndOrientation, null);

			this.AllNodes.Add(StartRRTNode);
			//Do n iterations of the algorithm
			double PreviousProgress = 0;
			GenerateStartLine ();
			Console.WriteLine ();
			for (UInt32 it = 0; it < Iterations; it++)
			{
				
				DoStep();

				Progress = (int)(Math.Round(((double)it / (double)Iterations)*100));
				if (Progress != PreviousProgress) {
					PreviousProgress = Progress;
					PrintProgress ();
				}
			}
			if (Finished != null)
				Finished(this, new EventArgs());
		}
		private void GenerateStartLine()
		{
			//TODO calculate distance
			double Distance = 1000;

			for (int offset = (int)-MaximumDrift; offset < (int)MaximumDrift; offset++) {
				
			
				RRTNode lastFound = null;
				for (int i = 0; i < Distance; i = i + StepWidth) {
					int NewX = StartRRTNode.Position.X + (int)((double)i * Math.Cos ((StartRRTNode.Orientation + offset) * RRTHelpers.ToRadians));
					int NewY = StartRRTNode.Position.Y + (int)((double)i * Math.Sin ((StartRRTNode.Orientation+ offset) * RRTHelpers.ToRadians));
					if (!PointValid (new Point ((int)NewX, (int)NewY))) {
						if (lastFound == null) {
							RRTNode NewNode = new RRTNode (new Point (NewX, NewY), StartRRTNode.Orientation + offset, StartRRTNode);
							StartRRTNode.AddSucessor (NewNode);
							this.AllNodes.Add (NewNode);
							lastFound = NewNode;
						} else {
							RRTNode NewNode = new RRTNode (new Point (NewX, NewY), StartRRTNode.Orientation+ offset, lastFound);
							lastFound.AddSucessor (NewNode);
							this.AllNodes.Add (NewNode);
							lastFound = NewNode;
						}
					} else
						break;

				}
				lastFound = null;
				for (int i = 0; i < Distance; i = i+ StepWidth) {
					int NewX = StartRRTNode.Position.X + (int)((double)i * Math.Cos (RRTHelpers.InvertOrientation (StartRRTNode.Orientation + offset) * RRTHelpers.ToRadians));
					int NewY = StartRRTNode.Position.Y + (int)((double)i * Math.Sin (RRTHelpers.InvertOrientation (StartRRTNode.Orientation +offset) * RRTHelpers.ToRadians));
					if (!PointValid (new Point ((int)NewX, (int)NewY))) {
						if (lastFound == null) {
							RRTNode NewNode = new RRTNode (new Point (NewX, NewY), StartRRTNode.Orientation + offset, StartRRTNode);
							NewNode.Inverted = true;
							StartRRTNode.AddSucessor (NewNode);
							this.AllNodes.Add (NewNode);
							lastFound = NewNode;
						} else {
							RRTNode NewNode = new RRTNode (new Point (NewX, NewY), StartRRTNode.Orientation +offset, lastFound);
							lastFound.AddSucessor (NewNode);
							this.AllNodes.Add (NewNode);
							lastFound = NewNode;
							NewNode.Inverted = true;
						}
					} else
						break;

				}

			}

		}
		/// <summary>
		/// Do a step into the right direction
		/// </summary>
		private void DoStep()
		{
			bool Curve = RRTHelpers.ShallInvertOrientation(this.PreferStraight);
			//Select a random base node from the list of all nodes
			RRTNode RandomNode = RRTHelpers.SelectRandomNode(AllNodes);

			//Produces very strange results sometimes
			bool SelectNearest = RRTHelpers.ShallInvertOrientation (100);
			if (SelectNearest) {
				//Take from 100 nodes the node that is the nearest to the endpoint
				double bestDistance = RRTHelpers.CalculateDistance (RandomNode, EndRRTNode);
				for (int i = 0; i < 10; i++) {
					RRTNode NewNode = RRTHelpers.SelectRandomNode (AllNodes);
					double distance = RRTHelpers.CalculateDistance (NewNode, EndRRTNode);
					if (distance < bestDistance) {
						bestDistance = distance;
						RandomNode = NewNode;
					}
				}
			}
			if (!Curve)
			{
				//First go straight


				//Get a new straight or drift random node
				RRTNode NewStraightNode = RRTHelpers.GetRandomStraightPoint(RandomNode, this.MaximumDrift, this.StraightInvertProbability);
				//Now step to the new node
				StepToNodeStraight(RandomNode, NewStraightNode);

			}
			else
			{
				//Second go curve


				double Distance = 0;
				double Angle = 0;
				double BaseAngle = 0;
				bool Left = false;
				Point Middle = new Point();
				RRTNode NewCurveNode = RRTHelpers.GetRandomCurvePoint(RandomNode, this.MinumumRadius, ref Distance, ref Angle, ref BaseAngle, ref Middle, ref Left);
				//RRTHelpers.DrawImportantNode(NewCurveNode, InternalMap, 4, Color.CornflowerBlue);
				StepToNodeCurve(RandomNode, NewCurveNode, Distance, Angle, BaseAngle, Middle, Left);
			}
		}
		/// <summary>
		/// Steps to node.
		/// Takes a start and an end node and the argument wether to go straight or in a circle.
		/// Steps into this direction
		/// </summary>
		/// <param name="Start">Start.</param>
		/// <param name="End">End.</param>
		private void StepToNodeStraight(RRTNode Start, RRTNode End)
		{

			//Linear equation between points: y = mx +b
			double m = ((double)Start.Position.Y - (double)End.Position.Y) / ((double)Start.Position.X - (double)End.Position.X);
			double b = (double)Start.Position.Y - m * (double)Start.Position.X;
			RRTNode lastFoundNode = null;

			//Lambda function that calculates a new point from a given x value
			//Checks if the node is valid 
			//Adds it into the list of nodes.
			//Returns false if point not valid 
			Func<double, bool> CalculateNewPoint = (double x) =>
			{
				double y = m * x + b;
				if (!PointValid(new Point((int)x, (int)y)))
				{
					if (lastFoundNode == null)
					{
						RRTNode BetweenNode = new RRTNode(new Point((int)x, (int)y), Start.Orientation, Start);
						Start.AddSucessor(BetweenNode);
						lastFoundNode = BetweenNode;
						BetweenNode.Inverted = End.Inverted;
						this.AllNodes.Add(BetweenNode);

					}

					else
					{
						RRTNode BetweenNode = new RRTNode(new Point((int)x, (int)y), lastFoundNode.Orientation, lastFoundNode);
						lastFoundNode.AddSucessor(BetweenNode);
						lastFoundNode = BetweenNode;
						BetweenNode.Inverted = End.Inverted;
						this.AllNodes.Add(BetweenNode);

						//Console.WriteLine(BetweenNode.ToString());
					}
					return true;
				}
				else
					return false;

			};

			//Step with "StepWidth" from start x to end x (Or if the StartPosition is > then the EndPosition the other way round
			if (Start.Position.X < End.Position.X)
			{
				for (double x = Start.Position.X; x < End.Position.X; x += StepWidth)
				{
					if (!CalculateNewPoint(x)) //Break if a not valid point was stepped into
						break;
				}
			}
			else
			{
				for (double x = Start.Position.X; x > End.Position.X; x -= StepWidth)
				{
					if (!CalculateNewPoint(x))
						break;
				}
			}


		}
		/// <summary>
		/// Steps to random node in a curve.
		/// </summary>
		/// <param name="Start">Start.</param>
		/// <param name="End">End.</param>
		/// <param name="Distance">Distance.</param>
		/// <param name="Angle">Angle.</param>
		/// <param name="BaseAngle">Base angle.</param>
		/// <param name="Middle">Middle.</param>
		/// <param name="Left">If set to <c>true</c> left.</param>
		private void StepToNodeCurve(RRTNode Start, RRTNode End, double Distance, double Angle, double BaseAngle, Point Middle, bool Left)
		{
			RRTNode lastFoundNode = null;
			//RRTHelpers.DrawImportantNode(new RRTNode(Middle, BaseAngle,null), InternalMap, 5, Color.Coral);

			Func<double, bool> CalculateNewPoint = (double x) =>
			{

				//We interpret the random angle as the angle in a polar coordinate system

				int NewX = Middle.X + (int)((double)Distance * Math.Cos((x) * RRTHelpers.ToRadians));
				int NewY = Middle.Y + (int)((double)Distance * Math.Sin((x) * RRTHelpers.ToRadians));


				double Orientation = Start.Orientation - (BaseAngle - x);

				Orientation = RRTHelpers.SanatizeAngle(Orientation);
				if (!PointValid(new Point((int)NewX, (int)NewY)))
				{
					if (lastFoundNode == null)
					{
						RRTNode BetweenNode = new RRTNode(new Point((int)NewX, (int)NewY), Orientation, Start);
						Start.AddSucessor(BetweenNode);
						lastFoundNode = BetweenNode;
						BetweenNode.Inverted = End.Inverted;
						this.AllNodes.Add(BetweenNode);

					}
					else
					{
						RRTNode BetweenNode = new RRTNode(new Point((int)NewX, (int)NewY), Orientation, lastFoundNode);
						lastFoundNode.AddSucessor(BetweenNode);
						lastFoundNode = BetweenNode;
						BetweenNode.Inverted = End.Inverted;
						this.AllNodes.Add(BetweenNode);

					}
					return true;
				}
				else
					return false;
			};

			double AdaptedStepWidth = (CircleStepWidth * 360.0 )/ (2 * Math.PI * Distance);
			if (Left)
			{
				for (double x = (BaseAngle) + AdaptedStepWidth; x < Angle; x += AdaptedStepWidth)
				{
					if (!CalculateNewPoint(x)) //Break if a not valid point was stepped into
						break;
				}
			}
			else
			{
				for (double x = (BaseAngle) - AdaptedStepWidth; x > Angle; x -= AdaptedStepWidth)
				{
					if (!CalculateNewPoint(x)) //Break if a not valid point was stepped into
						break;
				}
			}

		}
		/// <summary>
		/// Determins if the given point is valid
		/// </summary>
		/// <returns><c>true</c>, if valid was pointed, <c>false</c> otherwise.</returns>
		/// <param name="_Point">Point.</param>
		private bool PointValid(Point _Point)
		{
			return InternalMap.IsOccupied(_Point.X, _Point.Y);
		}

		public List<RRTPath> FindPathToTarget()
		{
			Rectangle TranslatedTargetArea = new Rectangle(EndPoint.X - TargetArea.Width / 2, EndPoint.Y - TargetArea.Height / 2, TargetArea.Width, TargetArea.Height);
			List<RRTNode> NodesInTargetArea = new List<RRTNode>();

			List<RRTPath> Paths = new List<RRTPath>();
			foreach (var item in AllNodes)
			{
				
				if (TranslatedTargetArea.Contains(item.Position) && Math.Abs(item.Orientation - EndRRTNode.Orientation) < AcceptableOrientationDeviation)
				{
					NodesInTargetArea.Add(item);
					Console.WriteLine("Found node in target area: " + item);

				}
			}
			if (NodesInTargetArea.Count == 0)
				return Paths;
			int B = 0;
			int R = 255;
			int Step = 255 / NodesInTargetArea.Count;
			foreach (var item in NodesInTargetArea)
			{
				double length = 0;
				RRTNode previous = item.Predecessor;


				RRTNode end = null;

				while (previous != null)
				{
					//RRTHelpers.DrawImportantNode(previous, InternalMap, 2, Color.FromArgb(R, 50, B));
					if (previous.Predecessor != null) {
						length += Math.Sqrt (Math.Pow (previous.Position.X - previous.Predecessor.Position.X, 2) + Math.Pow (previous.Position.Y - previous.Predecessor.Position.Y, 2));
						if (PointValid (previous.Position))
							Console.WriteLine ("WTF");
					}
					else
						end = previous;
					
					previous = previous.Predecessor;

					//path.CountNodes++;
				}
				RRTPath path = new RRTPath(item,end);
				Paths.Add(path);
				path.Color = Color.FromArgb(R, 50, B);
				//path.Length = length;
				path.DistanceToEnd = RRTHelpers.CalculateDistance(path.Start, EndRRTNode);
				path.OrientationDeviation = path.Start.Orientation - EndRRTNode.Orientation;
				B += Step;
				R -= Step;
			}

			List<RRTPath> SortedList = Paths.AsParallel().OrderBy(o => o.Cost()).ToList();
			foreach (var item in SortedList)
			{
				Console.WriteLine("Length for path " + item.Color.ToString() + " : " + item.Length + " Distance to End: " +item.DistanceToEnd + " OrientationDif: " + item.OrientationDeviation);
			}


			return SortedList;
		}
		void PrintProgress()
		{
			Console.SetCursorPosition (0, Console.CursorTop-1);

			Console.WriteLine ("Progress: " + Progress + "%");

		}
	}
}

