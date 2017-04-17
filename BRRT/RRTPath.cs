using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BRRT
{
	public class RRTPath
	{
		/// <summary>
		/// Path length.
		/// </summary>
		/// <value>The length.</value>
		public double Length ;
		/// <summary>
		/// The start point. ATTENTION Start node of a path is in this implementation the point nearest to the EndPoint
		/// </summary>
		public RRTNode Start;
		/// <summary>
		/// The end point. Usually the Start node where our robot starts
		/// </summary>
		public RRTNode End;
		/// <summary>
		/// The distance to EndPoint
		/// </summary>
		public double DistanceToEnd;
		/// <summary>
		/// Angle between End orientation and EndPoint orientation
		/// </summary>
		public double OrientationDeviation;
		/// <summary>
		/// The color we use for drawing.
		/// </summary>
		public System.Drawing.Color Color;
		/// <summary>
		/// The amount of nodes in this path.
		/// </summary>
		public int CountNodes;
		/// <summary>
		/// The list with all nodes.
		/// </summary>
		public List<RRTNode> NodesList;

		public RRTPath(RRTNode _Start,RRTNode _End)
		{
			this.Start = _Start;
			this.End = _End;

			//Calculate length and amount of nodes
			CalculateLength();
		}
		/// <summary>
		/// Calculates the length and the amount of nodes in the path.
		/// </summary>
		public void CalculateLength()
		{
			Length = 0;
			CountNodes = 0;
			RRTNode previous = Start;

			while (previous != null) {
				if(previous.Predecessor != null)
					Length += RRTHelpers.CalculateDistance(previous, previous.Predecessor);
				previous = previous.Predecessor;
				CountNodes++;
			}
			//Put them into the list.
			NodesList = ToList ();
		}
		/// <summary>
		/// Cost of this path. You can weight different properites differently by changing the values they are multiplied with.
		/// </summary>
		public double Cost()
		{
			return 2* Length + 0.5* DistanceToEnd + OrientationDeviation;
		}
		/// <summary>
		/// Serialize the node list into a file with the name path.
		/// </summary>
		/// <param name="Path">Path.</param>
		public void SaveToFile(string Path)
		{
			XmlSerializer ser = new XmlSerializer(NodesList.GetType());
			System.IO.TextWriter writer = new System.IO.StreamWriter (Path);
			Console.WriteLine ("Saving path to: " + Path);
			ser.Serialize (writer, NodesList);
		}
		/// <summary>
		/// Take the nodes in this path and put them into a list. Used for easier iteration.
		/// </summary>
		/// <returns>The list.</returns>
		public List<RRTNode> ToList()
		{
			List<RRTNode> nodes = new List<RRTNode> ();

			RRTNode node = Start;
			while (node != null) {
				nodes.Add (node);
				node = node.Predecessor;
			}
			return nodes;
		}
		/// <summary>
		/// Cleans the path. Removes all not needed entries from the Path.
		/// The returned path is a copy of the orignal nodes!
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="Path">Path.</param>
		public static RRTPath CleanPath(RRTPath Path)
		{
			List<RRTNode> nodes = Path.ToList ();
			//Clone all nodes
			List<RRTNode> clonedNodes = new List<RRTNode>();
			foreach (var item in nodes) {
				clonedNodes.Add (item.Clone ());
			}
			//Reconnect the nodes
			//First item is end
			//Last item is start
			RRTNode start = clonedNodes[0];
			RRTNode end = clonedNodes[clonedNodes.Count-1];

			for (int i = 0; i < clonedNodes.Count-1; i++) {
				clonedNodes [i].Predecessor = clonedNodes [i + 1];
				clonedNodes[i+1].AddSucessor(clonedNodes[i]);
			}
			RRTPath cleanedPath = new RRTPath (start,end);
			cleanedPath.Color = Path.Color;
			cleanedPath.DistanceToEnd = Path.DistanceToEnd;
			cleanedPath.OrientationDeviation = Path.OrientationDeviation;
			return cleanedPath;
		}
		/// <summary>
		/// Selects the node by the given index.
		/// </summary>
		/// <returns>The node.</returns>
		/// <param name="index">Index.</param>
		public RRTNode SelectNode(int index)
		{
			List<RRTNode> nodes = NodesList;
			if (index < nodes.Count)
				return nodes [index];
			throw new IndexOutOfRangeException ();
		}

	}
}
