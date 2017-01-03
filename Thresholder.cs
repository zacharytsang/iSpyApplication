using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace iSpyApplication
{
    public sealed partial class Thresholder : Panel
    {
        public Point RectStart = Point.Empty;
        public Point RectStop = Point.Empty;
        private bool _bMouseDown;
        private Rectangle _areaRectangle = Rectangle.Empty;
        
        public string Area
        {
            get
            {
                if (_areaRectangle == Rectangle.Empty)
                    return "";
                return _areaRectangle.X + "," + _areaRectangle.Y + "," + _areaRectangle.Width + "," +
                       _areaRectangle.Height;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _areaRectangle = Rectangle.Empty;
                else
                {
                    string[] vals = value.Split(',');
                    _areaRectangle = new Rectangle(
                        Convert.ToInt32(vals[0]),
                        Convert.ToInt32(vals[1]),
                        Convert.ToInt32(vals[2]),
                        Convert.ToInt32(vals[3])
                        );
                }
            }
        }

        private UnmanagedImage _lastFrame;
        public UnmanagedImage GrayImage;

        public Bitmap LastFrame
        {
            set
            {
                if (_lastFrame != null)
                    _lastFrame.Dispose();
                _lastFrame = UnmanagedImage.FromManagedImage(value);
                Invalidate();
            }
        }

        public void ClearRectangles()
        {
            _areaRectangle = Rectangle.Empty;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int startX = Convert.ToInt32((e.X * 1.0) / (Width * 1.0) * 100);
            int startY = Convert.ToInt32((e.Y * 1.0) / (Height * 1.0) * 100);
            if (startX > 100)
                startX = 100;
            if (startY > 100)
                startY = 100;
            RectStop = new Point(startX, startY);
            RectStart = new Point(startX, startY);
            _bMouseDown = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int endX = Convert.ToInt32((e.X * 1.0) / (Width * 1.0) * 100);
            int endY = Convert.ToInt32((e.Y * 1.0) / (Height * 1.0) * 100);
            if (endX > 100)
                endX = 100;
            if (endY > 100)
                endY = 100;
            RectStop = new Point(endX, endY);
            _bMouseDown = false;
            if (Math.Sqrt(Math.Pow(endX - RectStart.X, 2) + Math.Pow(endY - RectStart.Y, 2)) < 5)
            {
                RectStart = new Point(0, 0);
                RectStop = new Point(100, 100);
            }
            var start = new Point();
            var stop = new Point();
            
            start.X = RectStart.X;
            if (RectStop.X<RectStart.X)
                start.X = RectStop.X;
            start.Y = RectStart.Y;
            if (RectStop.Y<RectStart.Y)
                start.Y = RectStop.Y;

            stop.X = RectStop.X;
            if (RectStop.X<RectStart.X)
                stop.X = RectStart.X;
            stop.Y = RectStop.Y;
            if (RectStop.Y<RectStart.Y)
                stop.Y = RectStart.Y;

            var size = new Size(stop.X-start.X,stop.Y-start.Y);
            _areaRectangle = new Rectangle(start, size);
            RectStart = Point.Empty;
            RectStop = Point.Empty;
            OnBoundsChanged();
        }

        public event EventHandler BoundsChanged;

        private void OnBoundsChanged()
        {
            BoundsChanged(this, EventArgs.Empty);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_bMouseDown)
            {
                int endX = Convert.ToInt32((e.X * 1.0) / (Width * 1.0) * 100);
                int endY = Convert.ToInt32((e.Y * 1.0) / (Height * 1.0) * 100);
                if (endX > 100)
                    endX = 100;
                if (endY > 100)
                    endY = 100;

                RectStop = new Point(endX, endY);
            }
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _bMouseDown = false;
        }

        public Thresholder()
        {
            InitializeComponent();
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);
            Margin = new Padding(0, 0, 0, 0);
            Padding = new Padding(0, 0, 3, 3);
            BackgroundImageLayout = ImageLayout.Stretch;
        }


        protected override void OnPaint(PaintEventArgs pe)
        {
            // lock
            Monitor.Enter(this);
            
            var g = pe.Graphics;
            var col = Color.FromArgb(128, 255,255,255);
            var h = new SolidBrush(col);
            try
            {

                double wmultilocal = Convert.ToDouble(Width) / Convert.ToDouble(100);
                double hmultilocal = Convert.ToDouble(Height) / Convert.ToDouble(100);

                var rectLocal = new Rectangle(Convert.ToInt32(_areaRectangle.X * wmultilocal), Convert.ToInt32(_areaRectangle.Y * hmultilocal), Convert.ToInt32(_areaRectangle.Width * wmultilocal), Convert.ToInt32(_areaRectangle.Height * hmultilocal));

                if (_lastFrame != null)
                {
                    g.DrawImage(_lastFrame.ToManagedImage(), 0, 0, Width, Height);                   
                }
                
                
                if (_areaRectangle!=Rectangle.Empty)
                {
                    g.FillRectangle(h, rectLocal);
                }
                var p1 = new Point(Convert.ToInt32(RectStart.X * wmultilocal), Convert.ToInt32(RectStart.Y * hmultilocal));
                var p2 = new Point(Convert.ToInt32(RectStop.X * wmultilocal), Convert.ToInt32(RectStop.Y * hmultilocal));

                var ps = new[] { p1, new Point(p1.X, p2.Y), p2, new Point(p2.X, p1.Y), p1 };
                g.FillPolygon(h, ps);
                
            }
            catch
            {
            }
            
            h.Dispose();

            Monitor.Exit(this);

            base.OnPaint(pe);
        }
    }
}