using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class MainForm : Form
    {
        private readonly DataStore store;
        private readonly FlowLayoutPanel dailyFlow;
        private readonly FlowLayoutPanel projectFlow;
        private readonly Label progressLabel;
        private readonly Label greetingLabel;
        private readonly Label lunarLabel;
        private readonly Label yiLabel;
        private readonly Label jiLabel;
        private readonly Label dailyCountLabel;
        private readonly Label projectCountLabel;
        private readonly AccentProgressBar progressBar;
        private DateTime activeDate;
        private NotifyIcon trayIcon;
        private ToolStripMenuItem floatingTrayMenu;
        private FloatingReminderForm floatingReminder;
        private Timer dailyTimer;
        private bool allowExit;
        private bool trayBalloonShown;
        public bool RestartRequested { get; private set; }

        public MainForm(DataStore store)
        {
            this.store = store;
            activeDate = DateTime.Today;

            Text = "每日进度 · 必做与长期任务";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 650);
            Size = new Size(1080, 760);
            BackColor = Theme.Background;
            Font = Theme.BodyFont;
            Icon = Theme.GetAppIcon();
            Theme.ApplyWindowChrome(this);
            AutoScaleMode = AutoScaleMode.Dpi;

            TableLayoutPanel rootLayout = new TableLayoutPanel();
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.BackColor = Theme.Background;
            rootLayout.ColumnCount = 1;
            rootLayout.RowCount = 3;
            rootLayout.Margin = new Padding(0);
            rootLayout.Padding = new Padding(0);
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 192F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            Controls.Add(rootLayout);

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Theme.Card;
            header.Padding = new Padding(28, 18, 28, 14);

            greetingLabel = Theme.MakeLabel("今日进度", Theme.HeaderFont, Theme.Text);
            greetingLabel.Location = new Point(28, 18);
            header.Controls.Add(greetingLabel);

            Label dateLabel = Theme.MakeLabel(
                DateTime.Today.ToString("yyyy年M月d日 dddd"), Theme.BodyFont, Theme.Muted);
            dateLabel.Location = new Point(30, 60);
            header.Controls.Add(dateLabel);

            lunarLabel = Theme.MakeLabel("", Theme.SmallFont, Theme.Muted);
            lunarLabel.Location = new Point(30, 84);
            header.Controls.Add(lunarLabel);

            yiLabel = Theme.MakeLabel("", Theme.SmallFont, Theme.Success);
            yiLabel.AutoSize = false;
            yiLabel.Location = new Point(30, 108);
            yiLabel.Size = new Size(1000, 22);
            yiLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            header.Controls.Add(yiLabel);

            jiLabel = Theme.MakeLabel("", Theme.SmallFont, Theme.Danger);
            jiLabel.AutoSize = false;
            jiLabel.Location = new Point(30, 134);
            jiLabel.Size = new Size(1000, 22);
            jiLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            header.Controls.Add(jiLabel);

            progressLabel = Theme.MakeLabel("0 / 0", Theme.SectionFont, Theme.Primary);
            progressLabel.AutoSize = false;
            progressLabel.Size = new Size(280, 34);
            progressLabel.TextAlign = ContentAlignment.MiddleRight;
            progressLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressLabel.Location = new Point(770, 18);
            header.Controls.Add(progressLabel);

            progressBar = new AccentProgressBar();
            progressBar.Location = new Point(30, 169);
            progressBar.Height = 8;
            progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            progressBar.Width = 1000;
            header.Controls.Add(progressBar);

            header.Resize += delegate
            {
                progressLabel.Left = Math.Max(30, header.ClientSize.Width - progressLabel.Width - 30);
                progressBar.Width = Math.Max(100, header.ClientSize.Width - 60);
                yiLabel.Width = Math.Max(100, header.ClientSize.Width - 60);
                jiLabel.Width = Math.Max(100, header.ClientSize.Width - 60);
            };
            rootLayout.Controls.Add(header, 0, 0);

            RefreshCalendar(DateTime.Today);

            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Fill;
            content.BackColor = Theme.Background;
            content.Padding = new Padding(20, 18, 20, 12);
            content.ColumnCount = 2;
            content.RowCount = 1;
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

            dailyCountLabel = Theme.MakeLabel("0 / 0", Theme.SmallFont, Theme.Muted);
            projectCountLabel = Theme.MakeLabel("0 项进行中", Theme.SmallFont, Theme.Muted);
            dailyFlow = BuildSection(content, 0, "每日必做", dailyCountLabel, "＋ 新建每日任务", AddDailyTask);
            projectFlow = BuildSection(content, 1, "完成型任务", projectCountLabel, "＋ 新建长期任务", AddProject);
            rootLayout.Controls.Add(content, 0, 1);

            Panel footer = new Panel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor = Theme.Card;
            footer.Padding = new Padding(24, 8, 24, 8);

            FlowLayoutPanel footerActions = new FlowLayoutPanel();
            footerActions.Dock = DockStyle.Left;
            footerActions.AutoSize = true;
            footerActions.WrapContents = false;
            footerActions.BackColor = Theme.Card;
            footerActions.Margin = new Padding(0);

            Button historyButton = Theme.MakeButton("历史记录", false);
            historyButton.Width = 96;
            historyButton.Margin = new Padding(0, 0, 8, 0);
            historyButton.Click += delegate { new HistoryForm(store).ShowDialog(this); };
            footerActions.Controls.Add(historyButton);

            Button dataButton = Theme.MakeButton("数据位置", false);
            dataButton.Width = 96;
            dataButton.Margin = new Padding(0, 0, 8, 0);
            dataButton.Click += delegate
            {
                try { Process.Start("explorer.exe", "/select,\"" + store.DataFile + "\""); }
                catch { MessageBox.Show(store.DataFile, "数据文件位置"); }
            };
            footerActions.Controls.Add(dataButton);

            Button themeButton = Theme.MakeButton("外观 · " + Theme.CurrentName, false);
            themeButton.Width = 142;
            themeButton.Margin = new Padding(0);
            themeButton.Click += delegate
            {
                using (ThemeDialog dialog = new ThemeDialog(store.Data.ThemeName))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    if (string.Equals(dialog.SelectedTheme, store.Data.ThemeName,
                        StringComparison.OrdinalIgnoreCase)) return;
                    store.Data.ThemeName = dialog.SelectedTheme;
                    store.Save();
                    RestartRequested = true;
                    Close();
                }
            };
            footerActions.Controls.Add(themeButton);

            Button reminderButton = Theme.MakeButton("提醒设置", false);
            reminderButton.Width = 108;
            reminderButton.Margin = new Padding(8, 0, 0, 0);
            reminderButton.Click += delegate { OpenReminderSettings(); };
            footerActions.Controls.Add(reminderButton);
            footer.Controls.Add(footerActions);

            Label savedLabel = Theme.MakeLabel("所有更改都会自动保存到本机", Theme.SmallFont, Theme.Muted);
            savedLabel.Dock = DockStyle.Right;
            savedLabel.TextAlign = ContentAlignment.MiddleRight;
            footer.Controls.Add(savedLabel);
            rootLayout.Controls.Add(footer, 0, 2);

            InitializeReminderShell();
            FormClosing += HandleFormClosing;
            SizeChanged += delegate
            {
                if (WindowState == FormWindowState.Minimized && store.Data.MinimizeToTrayOnMinimize)
                    BeginInvoke((MethodInvoker)delegate { HideToTray(); });
            };
            Activated += delegate
            {
                if (activeDate != DateTime.Today)
                {
                    activeDate = DateTime.Today;
                    store.EnsureDay(activeDate);
                    store.Save();
                    RefreshCalendar(activeDate);
                    RefreshAll();
                }
            };

            Shown += delegate
            {
                ApplyReminderSettings();
                if (store.Data.StartMinimized) BeginInvoke((MethodInvoker)delegate { HideToTray(); });
            };

            dailyTimer = new Timer();
            dailyTimer.Interval = 60000;
            dailyTimer.Tick += delegate
            {
                if (activeDate == DateTime.Today) return;
                activeDate = DateTime.Today;
                store.EnsureDay(activeDate);
                store.Save();
                RefreshCalendar(activeDate);
                RefreshAll();
            };
            dailyTimer.Start();

            RefreshAll();
        }

        private void InitializeReminderShell()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = Theme.GetAppIcon();
            trayIcon.Text = "每日进度";
            trayIcon.Visible = true;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Font = Theme.BodyFont;
            menu.Items.Add("打开主界面", null, delegate { ShowMainWindow(); });
            floatingTrayMenu = new ToolStripMenuItem("显示悬浮提醒");
            floatingTrayMenu.Click += delegate
            {
                store.Data.FloatingReminderEnabled = !store.Data.FloatingReminderEnabled;
                store.Save();
                ApplyReminderSettings();
            };
            menu.Items.Add(floatingTrayMenu);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("退出每日进度", null, delegate { ExitApplication(); });
            trayIcon.ContextMenuStrip = menu;
            trayIcon.DoubleClick += delegate { ShowMainWindow(); };

            floatingReminder = new FloatingReminderForm(
                delegate { ShowMainWindow(); },
                delegate
                {
                    store.Data.FloatingReminderEnabled = false;
                    store.Save();
                    ApplyReminderSettings();
                },
                delegate { ExitApplication(); },
                delegate(Point location)
                {
                    store.Data.FloatingReminderX = location.X;
                    store.Data.FloatingReminderY = location.Y;
                    store.Save();
                });
            floatingReminder.TopMost = store.Data.FloatingReminderTopMost;
            UpdateFloatingTrayMenu();
        }

        private void ShowMainWindow()
        {
            if (!Visible) Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
            BringToFront();
        }

        private void HideToTray()
        {
            Hide();
            if (trayBalloonShown) return;
            trayBalloonShown = true;
            trayIcon.ShowBalloonTip(2500, "每日进度仍在运行",
                "双击右下角图标可恢复，右键可以退出程序。", ToolTipIcon.Info);
        }

        private void ExitApplication()
        {
            allowExit = true;
            Close();
        }

        private void HandleFormClosing(object sender, FormClosingEventArgs e)
        {
            bool systemIsClosing = e.CloseReason == CloseReason.WindowsShutDown ||
                e.CloseReason == CloseReason.ApplicationExitCall;
            if (!allowExit && !RestartRequested && !systemIsClosing &&
                store.Data.MinimizeToTrayOnClose)
            {
                e.Cancel = true;
                HideToTray();
                return;
            }

            store.Save();
            if (dailyTimer != null) dailyTimer.Stop();
            if (floatingReminder != null && !floatingReminder.IsDisposed) floatingReminder.Close();
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
        }

        private void ApplyReminderSettings()
        {
            if (floatingReminder == null || floatingReminder.IsDisposed) return;
            floatingReminder.TopMost = store.Data.FloatingReminderTopMost;
            if (store.Data.FloatingReminderEnabled)
            {
                if (!floatingReminder.Visible)
                    floatingReminder.ShowAtSavedPosition(store.Data.FloatingReminderX,
                        store.Data.FloatingReminderY);
            }
            else
            {
                floatingReminder.Hide();
            }
            UpdateFloatingTrayMenu();
        }

        private void UpdateFloatingTrayMenu()
        {
            if (floatingTrayMenu == null) return;
            floatingTrayMenu.Checked = store.Data.FloatingReminderEnabled;
            floatingTrayMenu.Text = store.Data.FloatingReminderEnabled ?
                "隐藏悬浮提醒" : "显示悬浮提醒";
        }

        private void OpenReminderSettings()
        {
            bool previousStartup = store.Data.StartWithWindows;
            using (ReminderSettingsDialog dialog = new ReminderSettingsDialog(store.Data))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                store.Data.StartWithWindows = dialog.StartWithWindows;
                store.Data.StartMinimized = dialog.StartMinimized;
                store.Data.MinimizeToTrayOnClose = dialog.MinimizeToTrayOnClose;
                store.Data.MinimizeToTrayOnMinimize = dialog.MinimizeToTrayOnMinimize;
                store.Data.FloatingReminderEnabled = dialog.FloatingReminderEnabled;
                store.Data.FloatingReminderTopMost = dialog.FloatingReminderTopMost;
            }

            if (previousStartup != store.Data.StartWithWindows)
            {
                string error;
                if (!StartupManager.SetEnabled(store.Data.StartWithWindows, out error))
                {
                    store.Data.StartWithWindows = previousStartup;
                    MessageBox.Show("无法修改开机启动设置：\r\n" + error,
                        "开机启动", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            store.Save();
            ApplyReminderSettings();
        }

        private FlowLayoutPanel BuildSection(TableLayoutPanel parent, int column, string title,
            Label countLabel, string buttonText, EventHandler click)
        {
            RoundedPanel card = new RoundedPanel();
            card.CornerRadius = 16;
            card.Dock = DockStyle.Fill;
            card.BackColor = Theme.Card;
            card.Margin = column == 0 ? new Padding(0, 0, 9, 0) : new Padding(9, 0, 0, 0);
            card.Padding = new Padding(18);

            TableLayoutPanel sectionLayout = new TableLayoutPanel();
            sectionLayout.Dock = DockStyle.Fill;
            sectionLayout.BackColor = Theme.Card;
            sectionLayout.ColumnCount = 1;
            sectionLayout.RowCount = 3;
            sectionLayout.Margin = new Padding(0);
            sectionLayout.Padding = new Padding(0);
            sectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            sectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            sectionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            sectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            card.Controls.Add(sectionLayout);

            Panel sectionHeader = new Panel();
            sectionHeader.Dock = DockStyle.Fill;
            sectionHeader.BackColor = Theme.Card;

            Label titleLabel = Theme.MakeLabel(title, Theme.SectionFont, Theme.Text);
            titleLabel.Location = new Point(0, 7);
            sectionHeader.Controls.Add(titleLabel);

            countLabel.Location = new Point(0, 31);
            sectionHeader.Controls.Add(countLabel);
            sectionLayout.Controls.Add(sectionHeader, 0, 0);

            Button addButton = Theme.MakeButton(buttonText, true);
            addButton.Dock = DockStyle.Fill;
            addButton.Margin = new Padding(0, 10, 0, 0);
            addButton.Click += click;
            sectionLayout.Controls.Add(addButton, 0, 2);

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            // Use a real vertical stack. Simulating one column with LeftToRight
            // wrapping leaves a stale horizontal layout range after the window
            // is enlarged and then reduced, which corrupts vertical scrolling.
            flow.FlowDirection = FlowDirection.TopDown;
            flow.WrapContents = false;
            flow.AutoScroll = true;
            flow.BackColor = Theme.Card;
            flow.Margin = new Padding(0);
            flow.Padding = new Padding(0, 6, 0, 10);
            flow.HorizontalScroll.Enabled = false;
            flow.HorizontalScroll.Visible = false;
            Theme.KeepVerticalScrollOnly(flow);
            flow.Resize += delegate
            {
                foreach (Control child in flow.Controls)
                    child.Width = GetFlowItemWidth(flow);
            };
            sectionLayout.Controls.Add(flow, 0, 1);

            parent.Controls.Add(card, column, 0);
            return flow;
        }

        private int GetFlowItemWidth(FlowLayoutPanel flow)
        {
            int scrollbarAllowance = SystemInformation.VerticalScrollBarWidth + 4;
            return Math.Max(100, flow.ClientSize.Width - flow.Padding.Horizontal - scrollbarAllowance);
        }

        private void RefreshAll()
        {
            store.SyncTodayWithTemplates();
            RefreshDaily();
            RefreshProjects();
            RefreshHeader();
        }

        private void RefreshDaily()
        {
            dailyFlow.SuspendLayout();
            dailyFlow.Controls.Clear();
            DailyDay today = store.EnsureDay(DateTime.Today);

            if (today.Items.Count == 0)
            {
                dailyFlow.Controls.Add(BuildEmpty("今天没有每日任务", "点击下方按钮添加运动、喝水等事项"));
            }
            else
            {
                foreach (DailyDayItem source in today.Items)
                {
                    DailyDayItem item = source;
                    dailyFlow.Controls.Add(BuildDailyRow(item));
                }
            }
            dailyFlow.ResumeLayout();
            RefreshHeader();
        }

        private Control BuildDailyRow(DailyDayItem item)
        {
            RoundedPanel row = new RoundedPanel();
            row.CornerRadius = 12;
            row.Height = 70;
            row.Width = GetFlowItemWidth(dailyFlow);
            row.BackColor = item.IsDone ? Theme.SurfaceAlt : Theme.PrimaryLight;
            row.Margin = new Padding(0, 0, 0, 8);
            row.Padding = new Padding(10);

            CheckBox check = new CheckBox();
            check.Checked = item.IsDone;
            check.Size = new Size(24, 24);
            check.Location = new Point(12, 21);
            check.Cursor = Cursors.Hand;
            row.Controls.Add(check);

            Label title = Theme.MakeLabel(item.Title, Theme.BodyFont, item.IsDone ? Theme.Muted : Theme.Text);
            title.Location = new Point(45, 12);
            title.MaximumSize = new Size(Math.Max(100, row.Width - 145), 24);
            if (item.IsDone) title.Font = new Font(Theme.BodyFont, FontStyle.Strikeout);
            row.Controls.Add(title);

            string subtitle = item.IsDone && item.CompletedAt.Length > 0
                ? "完成于 " + DateTime.Parse(item.CompletedAt).ToString("HH:mm")
                : "点击勾选完成";
            Label sub = Theme.MakeLabel(subtitle, Theme.SmallFont, Theme.Muted);
            sub.Location = new Point(46, 39);
            row.Controls.Add(sub);

            Button edit = Theme.MakeButton("编辑", false);
            edit.Size = new Size(58, 30);
            edit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            edit.Location = new Point(row.Width - 68, 18);
            edit.Font = Theme.SmallFont;
            edit.Click += delegate { EditDailyTask(item.TemplateId); };
            row.Controls.Add(edit);

            row.Resize += delegate
            {
                edit.Left = row.ClientSize.Width - 68;
                title.MaximumSize = new Size(Math.Max(100, row.ClientSize.Width - 145), 24);
            };

            check.CheckedChanged += delegate
            {
                item.IsDone = check.Checked;
                item.CompletedAt = check.Checked ? DateTime.Now.ToString("s") : "";
                store.Save();
                RefreshDaily();
            };
            return row;
        }

        private void RefreshProjects()
        {
            projectFlow.SuspendLayout();
            projectFlow.Controls.Clear();
            ProjectTask[] active = ProjectOrdering.Active(store.Data.Projects);

            projectCountLabel.Text = active.Length + " 项进行中";
            if (active.Length == 0)
            {
                projectFlow.Controls.Add(BuildEmpty("没有进行中的长期任务", "专利、文章、投稿等任务可以持续推进到完成"));
            }
            else
            {
                for (int index = 0; index < active.Length; index++)
                {
                    ProjectTask project = active[index];
                    projectFlow.Controls.Add(BuildProjectCard(project,
                        index > 0, index < active.Length - 1));
                }
            }
            projectFlow.ResumeLayout();
        }

        private Control BuildProjectCard(ProjectTask project, bool canMoveUp, bool canMoveDown)
        {
            RoundedPanel card = new RoundedPanel();
            card.CornerRadius = 14;
            card.Height = 142;
            card.Width = GetFlowItemWidth(projectFlow);
            card.BackColor = Theme.SurfaceAlt;
            card.Margin = new Padding(0, 0, 0, 10);
            card.Padding = new Padding(12);

            Label title = Theme.MakeLabel(project.Title, Theme.SectionFont, Theme.Text);
            title.Location = new Point(14, 12);
            title.MaximumSize = new Size(Math.Max(100, card.Width - 190), 28);
            card.Controls.Add(title);

            Label status = Theme.MakeLabel(project.Status, Theme.SmallFont,
                project.Status == "等待中" ? Theme.Warning : Theme.Primary);
            status.Location = new Point(15, 42);
            card.Controls.Add(status);

            string dueText = "无截止日期";
            Color dueColor = Theme.Muted;
            DateTime due;
            if (DateTime.TryParse(project.DueDate, out due))
            {
                int days = (due.Date - DateTime.Today).Days;
                dueText = days < 0 ? "已逾期 " + Math.Abs(days) + " 天" :
                    (days == 0 ? "今天截止" : days + " 天后截止");
                if (days <= 3) dueColor = days < 0 ? Theme.Danger : Theme.Warning;
            }
            Label dueLabel = Theme.MakeLabel(" · " + dueText, Theme.SmallFont, dueColor);
            dueLabel.Location = new Point(status.Right + 2, 42);
            card.Controls.Add(dueLabel);

            Label next = Theme.MakeLabel("下一步：" + project.NextStep(), Theme.SmallFont, Theme.Muted);
            next.Location = new Point(15, 69);
            next.MaximumSize = new Size(Math.Max(100, card.Width - 130), 22);
            card.Controls.Add(next);

            AccentProgressBar bar = new AccentProgressBar();
            bar.Location = new Point(15, 103);
            bar.Height = 8;
            bar.Width = Math.Max(100, card.Width - 175);
            bar.Value = Math.Max(0, Math.Min(100, project.ProgressPercent()));
            card.Controls.Add(bar);

            Label percent = Theme.MakeLabel(project.ProgressText(), Theme.SmallFont, Theme.Muted);
            percent.Location = new Point(card.Width - percent.Width - 15, 98);
            card.Controls.Add(percent);

            Button detail = Theme.MakeButton("打开", true);
            detail.Size = new Size(70, 34);
            detail.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            detail.Location = new Point(card.Width - 84, 15);
            detail.Font = Theme.SmallFont;
            detail.Click += delegate
            {
                new ProjectDetailForm(store, project).ShowDialog(this);
                RefreshAll();
            };
            card.Controls.Add(detail);

            Button moveDown = Theme.MakeButton("↓", false);
            moveDown.Size = new Size(34, 34);
            moveDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            moveDown.Location = new Point(card.Width - 124, 15);
            moveDown.Font = Theme.SectionFont;
            moveDown.Enabled = canMoveDown;
            moveDown.AccessibleName = "下移任务";
            moveDown.Click += delegate { MoveProject(project, 1); };
            card.Controls.Add(moveDown);

            Button moveUp = Theme.MakeButton("↑", false);
            moveUp.Size = new Size(34, 34);
            moveUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            moveUp.Location = new Point(card.Width - 164, 15);
            moveUp.Font = Theme.SectionFont;
            moveUp.Enabled = canMoveUp;
            moveUp.AccessibleName = "上移任务";
            moveUp.Click += delegate { MoveProject(project, -1); };
            card.Controls.Add(moveUp);

            card.Resize += delegate
            {
                detail.Left = card.ClientSize.Width - 84;
                moveDown.Left = card.ClientSize.Width - 124;
                moveUp.Left = card.ClientSize.Width - 164;
                title.MaximumSize = new Size(Math.Max(100, card.ClientSize.Width - 190), 28);
                next.MaximumSize = new Size(Math.Max(100, card.ClientSize.Width - 130), 22);
                bar.Width = Math.Max(100, card.ClientSize.Width - 175);
                percent.Left = card.ClientSize.Width - percent.Width - 15;
            };
            return card;
        }

        private void MoveProject(ProjectTask project, int direction)
        {
            if (!ProjectOrdering.Move(store.Data.Projects, project, direction)) return;
            store.Save();
            RefreshProjects();
        }

        private Control BuildEmpty(string title, string description)
        {
            RoundedPanel panel = new RoundedPanel();
            panel.CornerRadius = 14;
            panel.Height = 112;
            panel.Width = 320;
            panel.BackColor = Theme.SurfaceAlt;
            panel.Margin = new Padding(0, 8, 0, 8);

            Label titleLabel = Theme.MakeLabel(title, Theme.SectionFont, Theme.Text);
            titleLabel.Location = new Point(18, 25);
            panel.Controls.Add(titleLabel);

            Label desc = Theme.MakeLabel(description, Theme.SmallFont, Theme.Muted);
            desc.Location = new Point(19, 58);
            desc.MaximumSize = new Size(290, 42);
            panel.Controls.Add(desc);
            return panel;
        }

        private void RefreshHeader()
        {
            DailyDay today = store.EnsureDay(DateTime.Today);
            int total = today.Items.Count;
            int done = today.Items.Count(i => i.IsDone);
            dailyCountLabel.Text = done + " / " + total;
            progressLabel.Text = done + " / " + total + " 项每日必做";
            progressBar.Maximum = Math.Max(1, total);
            progressBar.Value = Math.Min(done, progressBar.Maximum);
            if (floatingReminder != null && !floatingReminder.IsDisposed)
                floatingReminder.UpdateProgress(done, total);

            if (total > 0 && done == total)
            {
                greetingLabel.Text = "🎉 今日必做已全部完成";
                greetingLabel.ForeColor = Theme.Success;
            }
            else
            {
                greetingLabel.Text = "今日进度";
                greetingLabel.ForeColor = Theme.Text;
            }
        }

        private void RefreshCalendar(DateTime date)
        {
            AlmanacEntry entry = AlmanacService.Get(date);
            lunarLabel.Text = entry.Lunar + "  ·  黄历民俗参考";
            yiLabel.Text = "宜  " + entry.Yi;
            jiLabel.Text = "忌  " + entry.Ji;
        }

        private void AddDailyTask(object sender, EventArgs e)
        {
            DailyTemplate template = new DailyTemplate
            {
                SortOrder = store.Data.DailyTemplates.Count == 0 ? 0 :
                    store.Data.DailyTemplates.Max(t => t.SortOrder) + 1
            };
            using (DailyTaskDialog dialog = new DailyTaskDialog(template, true))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                store.Data.DailyTemplates.Add(template);
                store.SyncTodayWithTemplates();
                RefreshAll();
            }
        }

        private void EditDailyTask(string templateId)
        {
            DailyTemplate template = store.Data.DailyTemplates.FirstOrDefault(t => t.Id == templateId);
            if (template == null) return;
            using (DailyTaskDialog dialog = new DailyTaskDialog(template, false))
            {
                DialogResult result = dialog.ShowDialog(this);
                if (result == DialogResult.Abort)
                {
                    if (MessageBox.Show("删除后不再生成此任务，过去的记录会保留。确定删除吗？",
                        "删除每日任务", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        template.IsArchived = true;
                }
                else if (result != DialogResult.OK) return;
                store.SyncTodayWithTemplates();
                RefreshAll();
            }
        }

        private void AddProject(object sender, EventArgs e)
        {
            ProjectTask project = new ProjectTask();
            project.SortOrder = ProjectOrdering.Next(store.Data.Projects);
            using (ProjectEditDialog dialog = new ProjectEditDialog(project, true))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                store.Data.Projects.Add(project);
                store.Save();
                new ProjectDetailForm(store, project).ShowDialog(this);
                RefreshAll();
            }
        }
    }
}
