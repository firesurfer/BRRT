using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

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
		public int CountNodes;
		public List<RRTNode> NodesList;

		public RRTPath(RRTNode _Start,RRTNode _End)
		{
			this.Start = _Start;
			this.End = _End;

			//Calculate length and amount of nodes
			CalculateLenght();
		}
		public RRTPath()
		{
		}
		public void CalculateLenght()
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
			NodesList = ToList ();
		}
		public double Cost()
		{
			return 2* Length + 0.5* DistanceToEnd + OrientationDeviation;
		}
		public void SaveToFile(string Path)
		{
			XmlSerializer ser = new XmlSerializer(NodesList.GetType());
			System.IO.TextWriter writer = new System.IO.StreamWriter (Path);
			Console.WriteLine ("Saving path to: " + Path);
			ser.Serialize (writer, NodesList);
		}

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
		/// Cleans the path. Removes all not needed entries form the Path - Ju
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
		public RRTNode SelectNode(int index)
		{
			List<RRTNode> nodes = NodesList;
			if (index < nodes.Count)
				return nodes [index];
			throw new IndexOutOfRangeException ();
		}
		//TODO add methods for calculating Length and so on
	}
}
