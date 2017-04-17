using System;
using System.Drawing;
using System.Collections.Generic;

namespace BRRT
{
	public static class RRTHelpers
	{
		static Random Randomizer = new Random (System.DateTime.Now.Millisecond);
		public const int MaximumDistance = 400;
		public const int MaximumCurveDistance = 300;
		public const double ToRadians = System.Math.PI / 180.0;
		public const double ToDegree = 180.0 / System.Math.PI;

		/// <summary>
		/// Re Seeds the Random class
		/// </summary>
		public static void ReSeedRandom ()
		{
			Randomizer = new Random (System.DateTime.Now.Millisecond);
		}

		/// <summary>
		/// Selects a random node from the list of all nodes.
		/// </summary>
		/// <returns>The random node.</returns>
		/// <param name="AllNodes">All nodes.</param>
		public static RRTNode SelectRandomNode (List<RRTNode> AllNodes)
		{
			int Max = AllNodes.Count;
			int RandomIndex = Randomizer.Next (Max);
			return AllNodes [RandomIndex];
		}

		/// <summary>
		/// Ask for a random value if the orientation should be used inverted. 
		/// By playing with the reference value you can decide how often we want to try to drive backwards instead of forwards
		/// </summary>
		/// <returns><c>true</c>, if orientation was inverted, <c>false</c> otherwise.</returns>
		public static bool BooleanRandom (int referenceValue)
		{
			if (Randomizer.Next (256) > referenceValue) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets a RandomNode starting from a BaseNode in a random distance and random drift angle
		/// </summary>
		/// <returns>The random straight point.</returns>
		/// <param name="BaseNode">Base node.</param>
		/// <param name="MaximumDrift">Maximum drift.</param>
		public static RRTNode GetRandomStraightPoint (RRTNode BaseNode, double MaximumDrift, int InvertProbability)
		{
			double Distance = Randomizer.NextDouble () * MaximumDistance;
			double Angle = (Randomizer.NextDouble () - 0.5) * 2 * MaximumDrift;
			double NewAngle;
			double Orientation = BaseNode.Orientation;

			bool Inverted = BooleanRandom (InvertProbability);

			if (Inverted) {
				NewAngle = InvertOrientation (BaseNode.Orientation) + Angle;
			} else {
				NewAngle = BaseNode.Orientation + Angle;
			}

			int NewX = (int)(BaseNode.Position.X + (Distance * Math.Cos (NewAngle * ToRadians)));
			int NewY = (int)(BaseNode.Position.Y + (Distance * Math.Sin (NewAngle * ToRadians)));

			Point NewPos = new Point (NewX, NewY);
			RRTNode NewNode = new RRTNode (NewPos, Orientation, null);
			NewNode.Inverted = Inverted;

			return NewNode;
		}

		/// <summary>
		/// Gets the random curve point.
		/// </summary>
		/// <returns>The random curve point.</returns>
		/// <param name="BaseNode">Base node.</param>
		/// <param name="MinimumRadius">Minimum radius.</param>
		/// <param name="_Distance">Distance.</param>
		/// <param name="_Angle">Angle.</param>
		public static RRTNode GetRandomCurvePoint (RRTNode BaseNode, double MinimumRadius, ref double _Distance, ref double _Angle, ref double _BaseAngle, ref Point _Middle, ref bool Left)
		{
			//Decide whether we want to have right or left turn
			//True = left , Right = false
			bool LeftOrRight = BooleanRandom (256 / 2);
			Left = LeftOrRight;

			//Get Random value for the distance between or choosen point and the middle of the circle. 
			double Distance = Randomizer.NextDouble () * MaximumCurveDistance + MinimumRadius;

			// Angle between 0 and 360.
			double Angle = (Randomizer.NextDouble ()) * 360;

			//Angle to our middle point (orthogonal to orientation)
			double AngleToMiddle = BaseNode.Orientation;
			if (Left)
				AngleToMiddle += 90;
			else
				AngleToMiddle -= 90;
			AngleToMiddle = SanatizeAngle (AngleToMiddle);

			//Calculate center point
			double MiddleX = BaseNode.Position.X + Math.Cos (AngleToMiddle * ToRadians) * Distance;
			double MiddleY = BaseNode.Position.Y + Math.Sin (AngleToMiddle * ToRadians) * Distance;


			Point Middle = new Point ((int)MiddleX, (int)MiddleY);

			double BaseAngle = 0;
			BaseAngle = Math.Atan2 (BaseNode.Position.Y - Middle.Y, BaseNode.Position.X - Middle.X) * ToDegree;
			BaseAngle = SanatizeAngle (BaseAngle);

			//Calculate new point
			int NewX = 0;
			int NewY = 0;

			NewX = Middle.X + (int)((double)Distance * Math.Cos ((Angle) * ToRadians));
			NewY = Middle.Y + (int)((double)Distance * Math.Sin ((Angle) * ToRadians));
			_Angle = Angle;

			//We drive backwarts if Angle > Baseangle
			bool Inverted = (InvertOrientation (BaseAngle) > Angle) && (Angle > BaseAngle); //^ BaseNode.Inverted;


			double NewOrientation = 0;
			NewOrientation = BaseNode.Orientation - (BaseAngle - Angle);
			
			NewOrientation = SanatizeAngle (NewOrientation);
			//Console.WriteLine("Orientation: " + NewOrientation);
			_Distance = Distance;

			_Middle = Middle;
			_BaseAngle = BaseAngle;

			RRTNode Node = new RRTNode (new Point (NewX, NewY), NewOrientation, null);
			Node.Inverted = Inverted;
			//Console.WriteLine(Node);
			//Console.WriteLine();
			return Node;

		}

		/// <summary>
		/// Sanatizes the angle into a range between 0 and 360.
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="Angle">Angle.</param>
		public static double SanatizeAngle (double Angle)
		{
			if (Angle < 0)
				return 360.0 + Angle;
			if (Angle > 360)
				return Angle - 360.0;
			return Angle;
		}

		/// <summary>
		/// Inverts the orientation.
		/// </summary>
		/// <returns>The orientation.</returns>
		/// <param name="Orientation">Orientation.</param>
		public static double InvertOrientation (double Orientation)
		{
			double NewOrientation = Orientation + 180;
			if (NewOrientation > 360)
				NewOrientation = NewOrientation - 360;
			return NewOrientation;
		}

		/// <summary>
		/// Draws the tree from the startpoint.
		/// </summary>
		/// <param name="Base">Base.</param>
		/// <param name="_Map">Map.</param>
		public static void DrawTree (RRTNode Base, Map _Map)
		{
			DrawImportantNode (Base, _Map, 5, Color.Red);
			Action<RRTNode> DrawAction = null;
			DrawAction = (RRTNode node) => {
				DrawImportantNode (node, _Map, 2, Color.Blue);
				foreach (var item in node.Successors) {
					Point position = _Map.ToMapCoordinates (node.Position);
					Point sucPosition = _Map.ToMapCoordinates (item.Position);
					Graphics g = Graphics.FromImage (_Map.ImageMap);
					g.DrawLine (Pens.Black, position, sucPosition);

				}
			};
			StepThroughTree (Base, DrawAction);
		}

		/// <summary>
		/// Helper method for iterating through the tree.
		/// </summary>
		/// <param name="Base">Base.</param>
		/// <param name="_Action">Action.</param>
		private static void StepThroughTree (RRTNode Base, Action<RRTNode> _Action)
		{
			foreach (var item in Base.Successors) {
				StepThroughTree (item, _Action);
				_Action (item);
			}
		}

		/// <summary>
		/// Draws the given RRTPath.
		/// </summary>
		/// <param name="Path">Path.</param>
		/// <param name="_Map">Map.</param>
		/// <param name="PathPen">Path pen.</param>
		public static void DrawPath (RRTPath Path, Map _Map, Pen PathPen)
		{
			RRTNode previous = Path.Start;
			Graphics g = Graphics.FromImage (_Map.ImageMap);
			while (previous != null) {
				RRTHelpers.DrawImportantNode (previous, _Map, 2, Path.Color);
				if (previous.Predecessor != null) {
					g.DrawLine (PathPen, _Map.ToMapCoordinates (previous.Position), _Map.ToMapCoordinates (previous.Predecessor.Position));
				}
				previous = previous.Predecessor;
			}
		}

		/// <summary>
		/// Draws the given Node + its orientation.
		/// </summary>
		/// <param name="Base">Base.</param>
		/// <param name="_Map">Map.</param>
		/// <param name="additional">Additional.</param>
		/// <param name="col">Col.</param>
		public static void DrawImportantNode (RRTNode Base, Map _Map, int additional, Color col)
		{
			Point position = _Map.ToMapCoordinates (Base.Position);
			for (int x = position.X - additional; x < position.X + additional; x++) {
				for (int y = position.Y - additional; y < position.Y + additional; y++) {
					_Map.DrawPixelOnBitmap (new Point (x, y), col);
				}
			}
			for (int i = 0; i < 15; i++) {
				if (!Base.Inverted) {
					int x = Base.Position.X + (int)(i * Math.Cos (Base.Orientation * ToRadians));
					int y = Base.Position.Y + (int)(i * Math.Sin (Base.Orientation * ToRadians));
					_Map.DrawPixelOnBitmap (_Map.ToMapCoordinates (new Point (x, y)), Color.DarkRed);  

				} else {
					int x = Base.Position.X + (int)(i * Math.Cos (Base.Orientation * ToRadians));
					int y = Base.Position.Y + (int)(i * Math.Sin (Base.Orientation * ToRadians));
					_Map.DrawPixelOnBitmap (_Map.ToMapCoordinates (new Point (x, y)), Color.DarkOliveGreen);
				}
			}
		}

		/// <summary>
		/// Calculates the distance between the two nodes.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		public static double CalculateDistance (RRTNode a, RRTNode b)
		{
			return Math.Sqrt (Math.Pow (a.Position.X - b.Position.X, 2) + Math.Pow (a.Position.Y - b.Position.Y, 2));
					
		}

		/// <summary>
		/// Calculates the angle from a to b. (a center of coordinate system)
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		public static double CalculateAngle (RRTNode a, RRTNode b)
		{
			return Math.Atan2 (b.Position.Y - a.Position.Y, b.Position.X - a.Position.X);
		}
	}


}

