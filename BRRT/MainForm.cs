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

		public MainForm ()
		{
			this.Height = 800;
			this.Width = 1200;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "Form1";
            this.Text = "Form1";
			Box.Size = new Size(this.Width, this.Height);
			Box.SizeMode = PictureBoxSizeMode.Zoom;
			Box.Location = new Point(0, 0);
			this.Controls.Add(Box);

             this.ResumeLayout(false);
            this.PerformLayout();
		}
		public void ShowImage(Image img)
		{
			Box.Image = img;

		}
	}
}

