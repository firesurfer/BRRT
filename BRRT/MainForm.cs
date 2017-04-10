using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
namespace BRRT
{
	public class MainForm : Form
	{
		PictureBox Box = new PictureBox();
		TreeView Tree = new TreeView();
		Map myMap;
		Point currentPointToDraw;
		Point lastPointToDraw;
		public MainForm()
		{
			this.WindowState = FormWindowState.Maximized;
			this.Width = Screen.GetWorkingArea(this).Width;
			this.Height = Screen.GetWorkingArea(this).Height;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

			this.Name = "Form1";
			this.Text = "Form1";
			Box.Size = new Size(this.Width - 400, this.Height);
			Box.SizeMode = PictureBoxSizeMode.Normal;
			Box.Location = new Point(0, 0);


			Tree.Location = new Point(Box.Size.Width, 0);
			Tree.Size = new Size(this.Width - Box.Size.Width, this.Height);
			Tree.NodeMouseClick += (object sender, TreeNodeMouseClickEventArgs e) =>
			{
				RRTNode node = (RRTNode)e.Node.Tag;

				lastPointToDraw = currentPointToDraw;
				currentPointToDraw = myMap.ToMapCoordinates(node.Position);

				Image img = (Image)myMap.ImageMap.Clone();
				Box.Image = null;
				Graphics g = Graphics.FromImage(img);
				g.DrawRectangle(Pens.Lime, currentPointToDraw.X - 2, currentPointToDraw.Y - 2, 5,5);

				Box.Image = img;
		
				Box.Refresh();

			};



			this.Controls.Add(Box);
			this.Controls.Add(Tree);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		public void ShowImage(Map _Map)
		{
			myMap = _Map;
			Box.Image = (Image)_Map.ImageMap.Clone();

		}

		public void CreateTree(RRTNode BaseNode)
		{


			TreeNode node = new TreeNode(BaseNode.Position + " : " + BaseNode.Orientation + " : " + BaseNode.Inverted); 
			node.Tag = BaseNode;
			Tree.Nodes.Add(node);

			Tree.BeginUpdate();
			BuildTree(BaseNode,node);
			//Tree.ExpandAll();
			Tree.EndUpdate();


		}
		private void BuildTree(RRTNode node, TreeNode treeNode)
		{
			List<TreeNode> nodes = new List<TreeNode>();
			foreach (var item in node.Successors)
			{
				TreeNode childNode = new TreeNode(item.Position + " : " + item.Orientation  + " : " + item.Inverted);
				childNode.Tag = item;
				nodes.Add(childNode);

				BuildTree(item, childNode);
			}
			treeNode.Nodes.AddRange(nodes.ToArray());
		}



	}
}

