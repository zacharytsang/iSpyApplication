using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using iSpyApplication.Properties;
using PictureBox = AForge.Controls.PictureBox;

namespace iSpyApplication
{
    public partial class FloorPlanControl : PictureBox
    {
        #region Public

        public bool NeedSizeUpdate;
        public bool ResizeParent;
        public objectsFloorplan Fpobject;
        public double LastAlertTimestamp = 0;
        public double LastRefreshTimestamp = 0;
        public int LastOid;
        public int LastOtid;
        public bool IsAlert;
        public Rectangle RestoreRect = Rectangle.Empty;
        private Image _imgplan, _imgview;
        public Image ImgPlan
        {
            
            get
            {
                if (_imgplan == null)
                    return null;
                
                return _imgplan;

            }
            set
            {
                lock(this)
                {
                    if (_imgview != null)
                        _imgview.Dispose();
                    if (_imgplan != null)
                        _imgplan.Dispose();
                    _imgplan = value;
                    _imgview = (Bitmap)_imgplan.Clone();
                }
            }
        }

        public Image ImgView
        {
             get
             {
                 return _imgview;
             }  
        }

        public MainForm Owner;
        public bool NeedsRefresh = true;

        #endregion


        #region SizingControls

        public void UpdatePosition()
        {
            Monitor.Enter(this);

            if (Parent != null && ImgPlan != null)
            {
                int width = ImgPlan.Width;
                int height = ImgPlan.Height;

                SuspendLayout();
                Size = new Size(width + 2, height + 26);
                ResumeLayout();
                NeedSizeUpdate = false;
            }
            Monitor.Exit(this);
        }

        private MousePos GetMousePos(Point location)
        {
            MousePos result = MousePos.NoWhere;
            int rightSize = Padding.Right;
            int bottomSize = Padding.Bottom;
            var testRect = new Rectangle(Width - rightSize, 0, Width - rightSize, Height - bottomSize);
            if (testRect.Contains(location)) result = MousePos.Right;
            testRect = new Rectangle(0, Height - bottomSize, Width - rightSize, Height);
            if (testRect.Contains(location)) result = MousePos.Bottom;
            testRect = new Rectangle(Width - rightSize, Height - bottomSize, Width, Height);
            if (testRect.Contains(location)) result = MousePos.BottomRight;
            return result;
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            IntPtr hwnd = Handle;
            if ((ResizeParent) && (Parent != null) && (Parent.IsHandleCreated))
            {
                hwnd = Parent.Handle;
            }
            MousePos mousePos = GetMousePos(e.Location);
            switch (mousePos)
            {
                case MousePos.Right:
                    {
                        NativeCalls.ReleaseCapture(hwnd);
                        NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeE, IntPtr.Zero);
                    }
                    break;
                case MousePos.Bottom:
                    {
                        NativeCalls.ReleaseCapture(hwnd);
                        NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeS, IntPtr.Zero);
                    }
                    break;
                case MousePos.BottomRight:
                    {
                        NativeCalls.ReleaseCapture(hwnd);
                        NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeSe, IntPtr.Zero);
                    }
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MousePos mousePos = GetMousePos(e.Location);
            switch (mousePos)
            {
                case MousePos.Right:
                    Cursor = Cursors.SizeWE;
                    break;
                case MousePos.Bottom:
                    Cursor = Cursors.SizeNS;
                    break;
                case MousePos.BottomRight:
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    Cursor = Cursors.Hand;
                    break;
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            if ((ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                double ar = Convert.ToDouble(MinimumSize.Width)/Convert.ToDouble(MinimumSize.Height);
                if (ImgPlan != null)
                    ar = Convert.ToDouble(ImgPlan.Width)/Convert.ToDouble(ImgPlan.Height);
                Width = Convert.ToInt32(ar*Height);
            }

            base.OnResize(eventargs);
            if (Width < MinimumSize.Width) Width = MinimumSize.Width;
            if (Height < MinimumSize.Height) Height = MinimumSize.Height;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Default;
        }

        #region Nested type: MousePos

        private enum MousePos
        {
            NoWhere,
            Right,
            Bottom,
            BottomRight
        }

        #endregion

        
        #endregion

        public FloorPlanControl(objectsFloorplan ofp, MainForm owner)
        {
            Owner = owner;
            InitializeComponent();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);

            Margin = new Padding(0, 0, 0, 0);
            Padding = new Padding(0, 0, 5, 5);
            BorderStyle = BorderStyle.FixedSingle;
            Fpobject = ofp;
            MouseClick += FloorPlanControlClick;
        }

        private void FloorPlanControlClick(object sender, MouseEventArgs e)
        {
            var local = new Point(e.X, e.Y);
            double xRat = Convert.ToDouble(Width)/533;
            double yRat = Convert.ToDouble(Height)/400;
            double hittargetw = 22*xRat;
            double hittargeth = 22*yRat;

            foreach (objectsFloorplanObjectsEntry fpoe in Fpobject.objects.@object)
            {
                if ((fpoe.x-hittargetw) * xRat <= local.X && (fpoe.x + hittargetw) * xRat > local.X &&
                    (fpoe.y-hittargeth) * yRat <= local.Y && (fpoe.y + hittargeth) * yRat > local.Y)
                {
                    switch (fpoe.type)
                    {
                        case "camera":
                            CameraWindow cw = Owner.GetCameraWindow(fpoe.id);
                            cw.Location = new Point(Location.X + e.X, Location.Y + e.Y);
                            cw.BringToFront();
                            break;
                        case "microphone":
                            VolumeLevel vl = Owner.GetMicrophone(fpoe.id);
                            vl.Location = new Point(Location.X + e.X, Location.Y + e.Y);
                            vl.BringToFront();
                            break;
                    }
                    break;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // lock
            Graphics gPlan = pe.Graphics;
            Rectangle rc = ClientRectangle;

            var grabPoints = new[]
                                    {
                                        new Point(rc.Width - 15, rc.Height), new Point(rc.Width, rc.Height - 15),
                                        new Point(rc.Width, rc.Height)
                                    };
            int textpos = rc.Height - 20;
            var drawFont = new Font(FontFamily.GenericSansSerif, 9);
            var grabBrush = new SolidBrush(Color.DarkGray);
            var drawBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));

            try
            {
                gPlan.FillPolygon(grabBrush, grabPoints);


                if (_imgview != null)
                {
                    lock (_imgview)
                    {
                        gPlan.DrawImage(_imgview, rc.X + 1, rc.Y + 1, rc.Width - 2, rc.Height - 26);
                        gPlan.DrawString(LocRm.GetString("FloorPlan") + ": " + Fpobject.name, drawFont,
                                    drawBrush,
                                    new PointF(5, textpos));
                    }
                }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }

            grabBrush.Dispose();
            drawBrush.Dispose();
            drawFont.Dispose();            

            base.OnPaint(pe);
        }

        

        public void Tick()
        {
            if (NeedSizeUpdate)
            {
                UpdatePosition();
            }

            if (NeedsRefresh)
            {
                bool alert = false;
                lock (this)
                {
                    if (_imgplan == null && Fpobject.image != "")
                    {
                        Image img = Image.FromFile(Fpobject.image);
                        var rf = new ResizeBicubic(533, 400);
                        _imgplan = rf.Apply((Bitmap)img);
                        _imgview = (Bitmap)_imgplan.Clone();
                    }
                    if (_imgplan == null)
                        return;

                    var alertBrush = new SolidBrush(Color.FromArgb(200, 255, 0, 0));
                    var noalertBrush = new SolidBrush(Color.FromArgb(200, 75, 172, 21));
                    var offlineBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));

                    var alertBrushScanner = new SolidBrush(Color.FromArgb(50, 255, 0, 0));
                    var noalertBrushScanner = new SolidBrush(Color.FromArgb(50, 75, 172, 21));
                    

                    Graphics gLf = Graphics.FromImage(_imgview);
                    gLf.DrawImage(_imgplan,0,0);

                    bool itemRemoved = false;
                    foreach (objectsFloorplanObjectsEntry fpoe in Fpobject.objects.@object)
                    {
                        var p = new Point(fpoe.x, fpoe.y);
                        if (fpoe.fov == 0)
                            fpoe.fov = 135;
                        if (fpoe.radius == 0)
                            fpoe.radius = 80;
                        switch (fpoe.type)
                        {
                            case "camera":
                                {
                                    var cw = Owner.GetCameraWindow(fpoe.id);
                                    if (cw != null)
                                    {
                                        double drad = (fpoe.angle - 180) * Math.PI / 180;
                                        var points = new[]
                                            {
                                                new Point(p.X + 11+Convert.ToInt32(20*Math.Cos(drad)), p.Y + 11 + Convert.ToInt32((20* Math.Sin(drad)))),
                                                new Point(p.X + 11+Convert.ToInt32(20*Math.Cos(drad+(135*Math.PI/180))), p.Y + 11 + Convert.ToInt32((20* Math.Sin(drad+(135*Math.PI/180))))),
                                                new Point(p.X + 11+Convert.ToInt32(10*Math.Cos(drad+(180*Math.PI/180))), p.Y + 11 + Convert.ToInt32((10* Math.Sin(drad+(180*Math.PI/180))))),
                                                new Point(p.X + 11+Convert.ToInt32(20*Math.Cos(drad-(135*Math.PI/180))), p.Y + 11 + Convert.ToInt32((20* Math.Sin(drad-(135*Math.PI/180)))))
                                            };
                                        if (cw.Camobject.settings.active && !cw.VideoSourceErrorState)
                                        {
                                            int offset = (fpoe.radius / 2) - 11;
                                            if (cw.Alerted)
                                            {
                                                gLf.FillPolygon(alertBrush, points);

                                                gLf.FillPie(alertBrushScanner, p.X - offset, p.Y - offset, fpoe.radius, fpoe.radius,
                                                            (float)(fpoe.angle - 180 - (fpoe.fov / 2)), fpoe.fov);
                                                alert = true;
                                            }
                                            else
                                            {
                                                gLf.FillPolygon(noalertBrush, points);
                                                gLf.FillPie(noalertBrushScanner, p.X - offset, p.Y - offset, fpoe.radius, fpoe.radius,
                                                            (float)(fpoe.angle - 180 - (fpoe.fov / 2)), fpoe.fov);
                                            }
                                        }
                                        else
                                        {
                                            gLf.FillPolygon(offlineBrush, points);
                                        }

                                    }
                                    else
                                    {
                                        fpoe.id = -2;
                                        itemRemoved = true;
                                    }
                                }
                                break;
                            case "microphone":
                                {
                                    var vw = Owner.GetMicrophone(fpoe.id);
                                    if (vw != null)
                                    {
                                        if (vw.Micobject.settings.active && !vw.NoSource)
                                        {
                                            if (vw.Alerted)
                                            {
                                                gLf.FillEllipse(alertBrush, p.X - 20, p.Y - 20, 40, 40);
                                                alert = true;
                                            }
                                            else
                                            {
                                                gLf.FillEllipse(noalertBrush, p.X - 15, p.Y - 15, 30, 30);
                                            }
                                        }
                                        else
                                        {
                                            gLf.FillEllipse(offlineBrush, p.X - 15, p.Y - 15, 30, 30);
                                        }
                                    }
                                    else
                                    {
                                        fpoe.id = -2;
                                        itemRemoved = true;
                                    }
                                }
                                break;
                        }
                    }

                    if (itemRemoved)
                    {
                        Fpobject.objects.@object = Fpobject.objects.@object.Where(fpoe => fpoe.id > -2).ToArray();
                    }
                    gLf.Dispose();
                    alertBrush.Dispose();
                    noalertBrush.Dispose();
                    alertBrushScanner.Dispose();
                    noalertBrushScanner.Dispose();
                    offlineBrush.Dispose();

                }
                Invalidate();
                LastRefreshTimestamp = DateTime.Now.UnixTicks();
                NeedsRefresh = false;
                IsAlert = alert;
            }
        }

        private static void FloorPlanResize(object sender, EventArgs e)
        {
        }

        //potential fix for "not enough memory" error (stack fault)
        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.Handle != null)
            {
                BeginInvoke(new EventHandler(HandleOnSizeChanged), this, e);
            }
            base.OnSizeChanged(e);
        }
        private void HandleOnSizeChanged(object sender, EventArgs e)
        {
            base.OnSizeChanged(e);
        }
        //end potential fix

        #region Nested type: ThreadSafeCommand

        public class ThreadSafeCommand : EventArgs
        {
            public string Command;
            // Constructor
            public ThreadSafeCommand(string command)
            {
                Command = command;
            }
        }

        #endregion
    }
}