using System;
namespace BRRT
{
	public class RRTPath
	{
		public double Length;
		public RRTNode Start;
		public RRTNode End;
		public double DistanceToEnd;
		public double OrientationDeviation;
		public System.Drawing.Color Color;
		public double Cost()
		{
			return 2* Length + 0.5* DistanceToEnd + OrientationDeviation;
		}
	}
}
