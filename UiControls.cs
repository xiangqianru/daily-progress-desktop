using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; }

        public RoundedPanel()
        {
            CornerRadius = 14;
            Resize += delegate { UpdateRegion(); };
        }

        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            if (Width <= 1 || Height <= 1) return;
            int radius = System.Math.Max(2, System.Math.Min(CornerRadius, System.Math.Min(Width, Height) / 2));
            int diameter = radius * 2;
            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle arc = new Rectangle(0, 0, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = Width - diameter - 1;
                path.AddArc(arc, 270, 90);
                arc.Y = Height - diameter - 1;
                path.AddArc(arc, 0, 90);
                arc.X = 0;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
                Region = new Region(path);
            }
        }
    }

    public class RoundedButton : Button
    {
        public int CornerRadius { get; set; }

        public RoundedButton()
        {
            CornerRadius = 10;
            Resize += delegate { UpdateRegion(); };
        }

        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            if (Width <= 1 || Height <= 1) return;
            int radius = System.Math.Max(2, System.Math.Min(CornerRadius, System.Math.Min(Width, Height) / 2));
            int diameter = radius * 2;
            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle arc = new Rectangle(0, 0, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = Width - diameter - 1;
                path.AddArc(arc, 270, 90);
                arc.Y = Height - diameter - 1;
                path.AddArc(arc, 0, 90);
                arc.X = 0;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
                Region = new Region(path);
            }
        }
    }

    public class AccentProgressBar : Control
    {
        private int minimum;
        private int maximum;
        private int currentValue;

        public Color TrackColor { get; set; }
        public Color FillColor { get; set; }

        public int Minimum
        {
            get { return minimum; }
            set { minimum = value; if (maximum <= minimum) maximum = minimum + 1; Value = currentValue; }
        }

        public int Maximum
        {
            get { return maximum; }
            set { maximum = System.Math.Max(minimum + 1, value); Value = currentValue; }
        }

        public int Value
        {
            get { return currentValue; }
            set
            {
                currentValue = System.Math.Max(minimum, System.Math.Min(maximum, value));
                Invalidate();
            }
        }

        public AccentProgressBar()
        {
            minimum = 0;
            maximum = 100;
            currentValue = 0;
            TrackColor = Theme.Border;
            FillColor = Theme.Primary;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle track = new Rectangle(0, 0, System.Math.Max(1, Width - 1), System.Math.Max(1, Height - 1));
            int radius = System.Math.Max(1, track.Height / 2);
            using (GraphicsPath path = MakePath(track, radius))
            using (SolidBrush brush = new SolidBrush(TrackColor))
                e.Graphics.FillPath(brush, path);

            double ratio = (currentValue - minimum) / (double)(maximum - minimum);
            int fillWidth = (int)System.Math.Round(track.Width * ratio);
            if (fillWidth <= 0) return;
            Rectangle fill = new Rectangle(track.X, track.Y, System.Math.Max(1, fillWidth), track.Height);
            using (GraphicsPath path = MakePath(fill, System.Math.Min(radius, fill.Width / 2)))
            using (SolidBrush brush = new SolidBrush(FillColor))
                e.Graphics.FillPath(brush, path);
        }

        private static GraphicsPath MakePath(Rectangle rectangle, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = System.Math.Max(2, radius * 2);
            if (rectangle.Width <= diameter || rectangle.Height <= diameter)
            {
                path.AddEllipse(rectangle);
                return path;
            }
            Rectangle arc = new Rectangle(rectangle.X, rectangle.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rectangle.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rectangle.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rectangle.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
