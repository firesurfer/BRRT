using System;
using System.Drawing;
using System.Collections.Generic;

namespace BRRT
{
	public class RRTNode
	{
		//Position of the Node
		public Point Position {get;set;}
		//Orientation of the robot at the Node
		public double Orientation{ get;set;}
		//Parent of this Node
		[System.Xml.Serialization.XmlIgnore()]
		public RRTNode Predecessor{ get; set;}
		//List of childs of this Node
		[System.Xml.Serialization.XmlIgnore()]
		public List<RRTNode> Successors{ get; private set;}
		/// <summary>
		/// Indicates whether the Orienation was used inverted in the algorithm. This is used for generation configurations that are driving backwards.
		/// </summary>
		/// <value><c>true</c> if inverted; otherwise, <c>false</c>.</value>
		public bool Inverted { get;  set; }
		public RRTNode (Point _Position, double _Orientation,RRTNode _Predecessor)
		{
			this.Position = _Position;
			this.Orientation = _Orientation;
			this.Predecessor = _Predecessor;
			this.Successors = new List<RRTNode> ();
		}
		/// <summary>
		/// Need for serialization.
		/// </summary>
		public RRTNode()
		{

		}
		/// <summary>
		/// Adds a sucessor to the list of successors
		/// </summary>
		/// <param name="Node">Node.</param>
		public void AddSucessor(RRTNode Node)
		{
			Successors.Add (Node);
		}
		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="BRRT.RRTNode"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="BRRT.RRTNode"/>.</returns>
		public override string ToString()
		{
			return string.Format("[RRTNode: Position={0}, Orientation={1}, Inverted={2}]", Position, Orientation, Inverted);
		}
		/// <summary>
		/// Clone this instance without copying predecessors and sucessors.
		/// </summary>
		public RRTNode Clone()
		{
			RRTNode temp = new RRTNode (this.Position, this.Orientation, null);
			temp.Inverted = this.Inverted;
			return temp;
		}
	}
}

