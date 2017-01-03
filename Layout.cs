using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace iSpyApplication
{
    class Layout:Panel
    {

        protected override void OnPaint(PaintEventArgs pe)
        {
            //Graphics gLayout = pe.Graphics;
            //var gridBrush = new SolidBrush(Color.White);
            //var gridPen = new Pen(gridBrush);
            //Rectangle rc = ClientRectangle;
            //int iControls = this.Controls.Count;
            //int ipow = 2;
            //while (Math.Pow(2,ipow)<iControls)
            //    ipow++;

            ////grid is ipow x ipow
            //int w = Width/ipow;
            //int h = Height/ipow;
            //int x = 0, y = 0;
            //while (x<Width-1)
            //{
            //    x += w;
            //    gLayout.DrawLine(gridPen,x,0,x,Height);
            //}
            //while (y < Height - 1)
            //{
            //    y += h;
            //    gLayout.DrawLine(gridPen, 0,y,Width,y);
            //}

            foreach (Control c in this.Controls)
            {
                if (c is CameraWindow)
                {
                    var cw = (CameraWindow) c;
                    var vc = cw.VolumeControl;
                    if (vc!=null)
                    {
                        vc.Location = new Point(c.Location.X,c.Location.Y+c.Height);
                        vc.Width = c.Width;
                        vc.Height = 40;
                    }

                }
            }
            base.OnPaint(pe);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Invalidate();
            base.OnSizeChanged(e);
        }
   }
}
