using System;
using System.Drawing;
using System.Collections.Generic;

namespace BRRT
{
	public static class RRTHelpers
	{
		static Random Randomizer = new Random(System.DateTime.Now.Second);
		public const int MaximumDistance = 2000;
		public const int MaximumCurveDistance = 200;
		public const double ToRadians = System.Math.PI / 180;
		public const double ToDegree = 180 / System.Math.PI;
		/// <summary>
		/// Re Seeds the Random class
		/// </summary>
		public static void ReSeedRandom()
		{
			Randomizer = new Random(System.DateTime.Now.Second);
		}
		/// <summary>
		/// Selects a random node from the list of all nodes.
		/// </summary>
		/// <returns>The random node.</returns>
		/// <param name="AllNodes">All nodes.</param>
		public static RRTNode SelectRandomNode(List<RRTNode> AllNodes)
		{
			int Max = AllNodes.Count;
			int RandomIndex = Randomizer.Next(Max);
			return AllNodes[RandomIndex];
		}
		/// <summary>
		/// Ask for a random value if the orientation should be used inverted. 
		/// By playing with the reference value you can decide how often we want to try to drive backwards instead of forwards
		/// </summary>
		/// <returns><c>true</c>, if orientation was inverted, <c>false</c> otherwise.</returns>
		public static bool ShallInvertOrientation(int referenceValue)
		{
			if (Randomizer.Next(256) > referenceValue)
			{
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
		public static RRTNode GetRandomStraightPoint(RRTNode BaseNode, double MaximumDrift)
		{
			double Distance = Randomizer.NextDouble() * MaximumDistance;
			double Angle = (Randomizer.NextDouble() - 0.5) * 2 * MaximumDrift;
			double NewAngle;
			double Orientation = BaseNode.Orientation;
			bool Inverted;
			if (ShallInvertOrientation(256 / 2))
			{
				NewAngle = InvertOrientation(BaseNode.Orientation) + Angle;
				Inverted = true;
			}
			else
			{
				NewAngle = BaseNode.Orientation + Angle;
				Inverted = false;
			}

			int NewX = (int)(BaseNode.Position.X + (Distance * Math.Cos(NewAngle * ToRadians)));
			int NewY = (int)(BaseNode.Position.Y + (Distance * Math.Sin(NewAngle * ToRadians)));

			Point NewPos = new Point(NewX, NewY);
			RRTNode NewNode = new RRTNode(NewPos, Orientation, null);
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
		public static RRTNode GetRandomCurvePoint(RRTNode BaseNode, double MinimumRadius, ref double _Distance, ref double _Angle, ref double _BaseAngle, ref Point _Middle, ref bool Left)
		{

			//Decide whether we want to have right or left turn
			//True = left , Right = false
			bool LeftOrRight = ShallInvertOrientation(256 / 2);
			Left = LeftOrRight;
			Left = true;
			//Get Random value for the distance between or choosen point and the middle of the circle. 

			double Distance = Randomizer.NextDouble() * MaximumCurveDistance + MinimumRadius;

			//Angle should be somewhere between 0 and 360
			double Angle = (Randomizer.NextDouble()) * 180;

			//Angle to our middle point (orthogonal to orientation)
			double AngleToMiddle = BaseNode.Orientation;
			if (Left)
				AngleToMiddle += 90;
			else
				AngleToMiddle -= 90;
			AngleToMiddle = SanatizeAngle(AngleToMiddle);



			Console.WriteLine("Left:" + Left);
			Console.WriteLine("Alpha: " + Angle);
			Console.WriteLine("AngleToMiddle: " + AngleToMiddle);
			Console.WriteLine("Distance: " + Distance);
			Console.WriteLine ("BaseNode: " + BaseNode);

			//Calculate center point
			double MiddleX = BaseNode.Position.X + Math.Cos(AngleToMiddle * ToRadians) * Distance;
			double MiddleY = BaseNode.Position.Y + Math.Sin(AngleToMiddle * ToRadians) * Distance;
		

			Point Middle = new Point((int)MiddleX, (int)MiddleY);
		
			
			double BaseAngle = 0;
			//Angle of the start point in a polar coordinate system centered around the middle
			if (BaseNode.Position.X != Middle.X) {
				BaseAngle = Math.Acos (((double)BaseNode.Position.X - MiddleX) / Distance) * ToDegree;
				if ((BaseNode.Position.X - MiddleX) / Distance >= 0.99)
					BaseAngle = 180;
				else if ((BaseNode.Position.X - MiddleX) / Distance <= -0.99)
					BaseAngle = 0;
			} else {
				BaseAngle = Math.Asin (((double)BaseNode.Position.Y - MiddleY / Distance)) * ToDegree;
				if ((BaseNode.Position.Y - MiddleY) / Distance >= 0.99)
					BaseAngle = 90;
				else if ((BaseNode.Position.Y - MiddleY) / Distance <= -0.99)
					BaseAngle = -90;
			}
			BaseAngle = SanatizeAngle (BaseAngle);

			//Calculate new point
			int	NewX = 0;
			int	NewY = 0;
			if (Left) {
					NewX = Middle.X + (int)((double)Distance * Math.Cos ((BaseAngle+Angle) * ToRadians));
					NewY = Middle.Y + (int)((double)Distance * Math.Sin ((BaseAngle+Angle) * ToRadians));
			} else {
					NewX = Middle.X + (int)((double)Distance * Math.Cos ((Angle) * ToRadians));
					NewY = Middle.Y + (int)((double)Distance * Math.Sin ((Angle) * ToRadians));
			}


			Console.WriteLine("BaseAngle: " + BaseAngle);
			//bool Inverted = (Angle < 0);
			double NewOrientation = BaseNode.Orientation + (BaseAngle - Angle);
			Console.WriteLine ("Orientation: " + NewOrientation);
			_Distance = Distance;
			_Angle = Angle + BaseAngle;
			_Middle = Middle;
			_BaseAngle = BaseAngle;
			RRTNode Node = new RRTNode(new Point(NewX, NewY), NewOrientation, null);
			//Node.Inverted = Inverted;
			Console.WriteLine(Node);
			return Node;




		}
		/// <summary>
		/// Sanatizes the angle into a range between 0 and 360.
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="Angle">Angle.</param>
		public static double SanatizeAngle(double Angle)
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
		public static double InvertOrientation(double Orientation)
		{
			double NewOrientation = Orientation + 180;
			if (NewOrientation > 360)
				NewOrientation = NewOrientation - 360;
			return NewOrientation;
		}
		public static void DrawTree(RRTNode Base, Map _Map)
		{
			DrawImportantNode(Base, _Map, 5, Color.Red);

			Action<RRTNode> DrawAction = null;
			DrawAction = (RRTNode node) =>
			{


				DrawImportantNode(node, _Map, 2, Color.Blue);
				foreach (var item in node.Successors)
				{
					Point position = _Map.ToMapCoordinates(node.Position);
					Point sucPosition = _Map.ToMapCoordinates(item.Position);
					double m = ((double)position.Y - (double)sucPosition.Y) / ((double)position.X - (double)sucPosition.X);
					double b = (double)position.Y - m * (double)position.X;

					if (position.X < sucPosition.X)
					{
						for (int x = position.X; x < sucPosition.X; x++)
						{
							double y = m * x + b;
							_Map.DrawPixelOnBitmap(new Point(x, (int)y), Color.Black);
						//_Map.DrawPixelOnBitmap(new Point(x+1,(int)y+1), Color.Black);
					}
					}
					else
					{
						for (int x = position.X; x > sucPosition.X; x--)
						{
							double y = m * x + b;

							_Map.DrawPixelOnBitmap(new Point(x, (int)y), Color.Black);
						//_Map.DrawPixelOnBitmap(new Point(x+1,(int)y+1), Color.Black);
					}
					}
				}

			};
			StepThroughTree(Base, DrawAction);
		}
		private static void StepThroughTree(RRTNode Base, Action<RRTNode> _Action)
		{
			foreach (var item in Base.Successors)
			{
				StepThroughTree(item, _Action);
				_Action(item);
			}
		}
		public static void DrawImportantNode(RRTNode Base, Map _Map, int additional, Color col)
		{
			Point position = _Map.ToMapCoordinates(Base.Position);
			for (int x = position.X - additional; x < position.X + additional; x++)
			{
				for (int y = position.Y - additional; y < position.Y + additional; y++)
				{
					_Map.DrawPixelOnBitmap(new Point(x, y), col);

				}
			}

			for (int i = 0; i < 10; i++)
			{
				if (!Base.Inverted)
				{
					//Console.WriteLine (Base.Orientation);
					Point MapPosition = _Map.ToMapCoordinates(Base.Position);
					int x = MapPosition.X + (int)(i * Math.Cos(Base.Orientation * ToRadians));
					int y = MapPosition.Y + (int)(i * Math.Sin(Base.Orientation * ToRadians));
					_Map.DrawPixelOnBitmap(new Point(x, y), Color.DarkRed);

				}
				else
				{
					Point MapPosition = _Map.ToMapCoordinates(Base.Position);
					int x = MapPosition.X + (int)(i * Math.Cos(InvertOrientation(Base.Orientation) * ToRadians));
					int y = MapPosition.Y + (int)(i * Math.Sin(InvertOrientation(Base.Orientation) * ToRadians));
					_Map.DrawPixelOnBitmap(new Point(x, y), Color.DarkRed);
				}
			}
		}
	}

}

