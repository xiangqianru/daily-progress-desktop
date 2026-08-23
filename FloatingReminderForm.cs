using System;
using System.Drawing;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class FloatingReminderForm : Form
    {
        private const int WsExToolWindow = 0x00000080;
        private const int WsExNoActivate = 0x08000000;
        private readonly Action openMain;
        private readonly Action disableReminder;
        private readonly Action exitApplication;
        private readonly Action<Point> positionCommitted;
        private readonly RoundedPanel avatarPanel;
        private readonly Label avatarLabel;
        private readonly Label titleLabel;
        private readonly Label subtitleLabel;
        private readonly AccentProgressBar progressBar;
        private bool dragging;
        private bool moved;
        private Point dragMouseOrigin;
        private Point dragWindowOrigin;

        public FloatingReminderForm(Action openMain, Action disableReminder,
            Action exitApplication, Action<Point> positionCommitted)
        {
            this.openMain = openMain;
            this.disableReminder = disableReminder;
            this.exitApplication = exitApplication;
            this.positionCommitted = positionCommitted;

            Text = "每日进度悬浮提醒";
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(292, 118);
            MinimumSize = Size;
            MaximumSize = Size;
            BackColor = Theme.Border;
            Padding = new Padding(1);
            TopMost = true;
            Font = Theme.BodyFont;
            Cursor = Cursors.Hand;
            AutoScaleMode = AutoScaleMode.Dpi;

            RoundedPanel root = new RoundedPanel();
            root.CornerRadius = 18;
            root.Dock = DockStyle.Fill;
            root.BackColor = Theme.Card;
            Controls.Add(root);

            avatarPanel = new RoundedPanel();
            avatarPanel.CornerRadius = 29;
            avatarPanel.Location = new Point(14, 16);
            avatarPanel.Size = new Size(58, 58);
            avatarPanel.BackColor = Theme.PrimaryLight;
            root.Controls.Add(avatarPanel);

            avatarLabel = Theme.MakeLabel("0", new Font("Microsoft YaHei UI", 18F, FontStyle.Bold), Theme.Primary);
            avatarLabel.AutoSize = false;
            avatarLabel.Dock = DockStyle.Fill;
            avatarLabel.TextAlign = ContentAlignment.MiddleCenter;
            avatarPanel.Controls.Add(avatarLabel);

            titleLabel = Theme.MakeLabel("今日提醒", Theme.SectionFont, Theme.Text);
            titleLabel.AutoSize = false;
            titleLabel.Location = new Point(86, 17);
            titleLabel.Size = new Size(188, 28);
            root.Controls.Add(titleLabel);

            subtitleLabel = Theme.MakeLabel("点击打开每日进度", Theme.SmallFont, Theme.Muted);
            subtitleLabel.AutoSize = false;
            subtitleLabel.Location = new Point(87, 47);
            subtitleLabel.Size = new Size(188, 24);
            root.Controls.Add(subtitleLabel);

            progressBar = new AccentProgressBar();
            progressBar.Location = new Point(16, 91);
            progressBar.Size = new Size(260, 7);
            root.Controls.Add(progressBar);

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Font = Theme.BodyFont;
            menu.Items.Add("打开主界面", null, delegate { openMain(); });
            menu.Items.Add("隐藏悬浮提醒", null, delegate { disableReminder(); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("退出每日进度", null, delegate { exitApplication(); });
            ContextMenuStrip = menu;

            WireInteraction(this);
            Resize += delegate { Region = CreateRoundedRegion(ClientRectangle, 18); };
            Shown += delegate { Region = CreateRoundedRegion(ClientRectangle, 18); };
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= WsExToolWindow | WsExNoActivate;
                return parameters;
            }
        }

        public void UpdateProgress(int done, int total)
        {
            int safeTotal = Math.Max(0, total);
            int safeDone = Math.Max(0, Math.Min(done, safeTotal));
            progressBar.Maximum = Math.Max(1, safeTotal);
            progressBar.Value = safeDone;

            if (safeTotal == 0)
            {
                avatarLabel.Text = "＋";
                avatarLabel.ForeColor = Theme.Primary;
                avatarPanel.BackColor = Theme.PrimaryLight;
                titleLabel.Text = "今天还没有必做事项";
                titleLabel.ForeColor = Theme.Text;
                subtitleLabel.Text = "点击添加一个每日任务";
                progressBar.FillColor = Theme.Primary;
            }
            else if (safeDone == safeTotal)
            {
                avatarLabel.Text = "✓";
                avatarLabel.ForeColor = Theme.Success;
                avatarPanel.BackColor = Theme.PrimaryLight;
                titleLabel.Text = "今天的任务完成啦";
                titleLabel.ForeColor = Theme.Success;
                subtitleLabel.Text = "已完成 " + safeDone + " / " + safeTotal + " · 点击查看";
                progressBar.FillColor = Theme.Success;
            }
            else
            {
                avatarLabel.Text = (safeTotal - safeDone).ToString();
                avatarLabel.ForeColor = Theme.Primary;
                avatarPanel.BackColor = Theme.PrimaryLight;
                titleLabel.Text = "今天还差 " + (safeTotal - safeDone) + " 项";
                titleLabel.ForeColor = Theme.Text;
                subtitleLabel.Text = "已完成 " + safeDone + " / " + safeTotal + " · 点击打开";
                progressBar.FillColor = Theme.Primary;
            }
        }

        public void ShowAtSavedPosition(int savedX, int savedY)
        {
            Screen screen = Screen.PrimaryScreen;
            Rectangle area = screen.WorkingArea;
            Point target = new Point(area.Right - Width - 24, area.Bottom - Height - 24);
            if (savedX >= 0 || savedY >= 0)
            {
                Point saved = new Point(savedX, savedY);
                Rectangle savedArea = Screen.FromPoint(saved).WorkingArea;
                if (savedX >= savedArea.Left && savedY >= savedArea.Top &&
                    savedX + Width <= savedArea.Right && savedY + Height <= savedArea.Bottom)
                    target = saved;
            }
            Location = target;
            Show();
        }

        private void WireInteraction(Control control)
        {
            control.ContextMenuStrip = ContextMenuStrip;
            control.MouseDown += HandleMouseDown;
            control.MouseMove += HandleMouseMove;
            control.MouseUp += HandleMouseUp;
            control.Click += HandleClick;
            foreach (Control child in control.Controls) WireInteraction(child);
        }

        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            dragging = true;
            moved = false;
            dragMouseOrigin = Cursor.Position;
            dragWindowOrigin = Location;
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging) return;
            Point current = Cursor.Position;
            int dx = current.X - dragMouseOrigin.X;
            int dy = current.Y - dragMouseOrigin.Y;
            if (Math.Abs(dx) + Math.Abs(dy) > 3) moved = true;
            Location = new Point(dragWindowOrigin.X + dx, dragWindowOrigin.Y + dy);
        }

        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            dragging = false;
            if (moved) positionCommitted(Location);
        }

        private void HandleClick(object sender, EventArgs e)
        {
            if (!moved) openMain();
            moved = false;
        }

        private static Region CreateRoundedRegion(Rectangle bounds, int radius)
        {
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                int diameter = radius * 2;
                Rectangle arc = new Rectangle(bounds.X, bounds.Y, diameter, diameter);
                path.AddArc(arc, 180, 90);
                arc.X = bounds.Right - diameter - 1;
                path.AddArc(arc, 270, 90);
                arc.Y = bounds.Bottom - diameter - 1;
                path.AddArc(arc, 0, 90);
                arc.X = bounds.X;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
                return new Region(path);
            }
        }
    }
}
