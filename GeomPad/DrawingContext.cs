using System;
using System.Drawing;
using System.Windows.Forms;

namespace GeomPad
{
    public class DrawingContext
    {
        public Graphics gr;
        public float scale = 1;


        public float startx, starty;
        public float origsx, origsy;
        public bool isDrag = false;
        public float sx, sy;
        public float zoom = 1;

        public Bitmap Bmp;

        public void UpdateDrag()
        {
            if (isDrag)
            {
                var p = PictureBox.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
        }
        public PointF GetCursor()
        {
            var p = PictureBox.PointToClient(Cursor.Position);
            var pn = BackTransform(p);
            return pn;
        }
        public void Init(PictureBox pb)
        {
            Init(new EventWrapperPictureBox(pb) { });
        }
        public void Init(EventWrapperPictureBox pb)
        {
            PictureBox = pb;
            pb.MouseWheelAction = PictureBox1_MouseWheel;
            pb.MouseUpAction = PictureBox1_MouseUp;
            pb.MouseDownAction = PictureBox1_MouseDown;

            pb.SizeChangedAction = Pb_SizeChanged;

            //pb.SizeChanged += Pb_SizeChanged;
            //pb.MouseWheel += PictureBox1_MouseWheel;
            //pb.MouseUp += PictureBox1_MouseUp;
            //pb.MouseDown += PictureBox1_MouseDown;
            //pb.MouseMove += PictureBox1_MouseMove;

            //Bmp = new Bitmap(pb.Control.Width, pb.Control.Height);
            //  gr = Graphics.FromImage(Bmp);
        }
        public float ZoomFactor = 1.5f;

        public virtual void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            //zoom *= Math.Sign(e.Delta) * 1.3f;
            //zoom += Math.Sign(e.Delta) * 0.31f;

            var pos = PictureBox.Control.PointToClient(Cursor.Position);
            if (!PictureBox.Control.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
            {
                return;
            }

            float zold = zoom;

            if (e.Delta > 0) { zoom *= ZoomFactor; } else { zoom /= ZoomFactor; }

            if (zoom < 0.0008) { zoom = 0.0008f; }
            if (zoom > 10000) { zoom = 10000f; }

            sx = -(pos.X / zold - sx - pos.X / zoom);
            sy = (pos.Y / zold + sy - pos.Y / zoom);
        }

        public virtual void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = PictureBox.Control.PointToClient(Cursor.Position);

            if (e.Button == MouseButtons.Right)
            {
                isDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
        }

        public virtual void Pb_SizeChanged(object sender, EventArgs e)
        {
            Bmp = new Bitmap(PictureBox.Control.Width, PictureBox.Control.Height);
            gr = Graphics.FromImage(Bmp);
        }
        public virtual void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
            var p = PictureBox.Control.PointToClient(Cursor.Position);



        }
        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, -(p1.Y + sy) * zoom);
        }
        public virtual PointF Transform(SvgPoint p1)
        {
            return new PointF((float)((p1.X + sx) * zoom), (float)(-(p1.Y + sy) * zoom));
        }

        public virtual PointF BackTransform(PointF p1)
        {
            var posx = (p1.X / zoom - sx);
            var posy = (-p1.Y / zoom - sy);
            return new PointF(posx, posy);
        }
        public EventWrapperPictureBox PictureBox;

        internal void FillEllipse(Brush black, float v1, float v2, float v3, float v4)
        {
            var pp = Transform(new PointF(v1, v2));
            gr.FillEllipse(black, pp.X, pp.Y, v3 * scale, v4 * scale);
        }

        internal void DrawLine(Pen black, PointF point, PointF point2)
        {
            var pp = Transform(point);
            var pp2 = Transform(point2);
            gr.DrawLine(black, pp, pp2);
        }
    }
}
