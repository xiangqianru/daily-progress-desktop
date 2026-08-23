using System.Drawing;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class ReminderSettingsDialog : Form
    {
        private readonly CheckBox startWithWindowsBox;
        private readonly CheckBox startMinimizedBox;
        private readonly CheckBox closeToTrayBox;
        private readonly CheckBox minimizeToTrayBox;
        private readonly CheckBox floatingEnabledBox;
        private readonly CheckBox floatingTopMostBox;

        public bool StartWithWindows { get { return startWithWindowsBox.Checked; } }
        public bool StartMinimized { get { return startMinimizedBox.Checked; } }
        public bool MinimizeToTrayOnClose { get { return closeToTrayBox.Checked; } }
        public bool MinimizeToTrayOnMinimize { get { return minimizeToTrayBox.Checked; } }
        public bool FloatingReminderEnabled { get { return floatingEnabledBox.Checked; } }
        public bool FloatingReminderTopMost { get { return floatingTopMostBox.Checked; } }

        public ReminderSettingsDialog(AppData data)
        {
            Text = "提醒与后台运行";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(630, 570);
            BackColor = Theme.Background;
            Font = Theme.BodyFont;
            Icon = Theme.GetAppIcon();
            Theme.ApplyWindowChrome(this);
            AutoScaleMode = AutoScaleMode.Dpi;

            Label title = Theme.MakeLabel("提醒与后台运行", Theme.HeaderFont, Theme.Text);
            title.Location = new Point(28, 23);
            Controls.Add(title);

            Label subtitle = Theme.MakeLabel("设置开机启动、最小化方式，以及桌面悬浮任务提醒。",
                Theme.SmallFont, Theme.Muted);
            subtitle.Location = new Point(30, 68);
            Controls.Add(subtitle);

            RoundedPanel startupCard = BuildCard("启动方式", 28, 105, 574, 118);
            startWithWindowsBox = MakeCheckBox("开机后自动启动每日进度", data.StartWithWindows, 20, 46);
            startMinimizedBox = MakeCheckBox("程序启动后自动隐藏到系统托盘", data.StartMinimized, 20, 78);
            startupCard.Controls.Add(startWithWindowsBox);
            startupCard.Controls.Add(startMinimizedBox);

            RoundedPanel trayCard = BuildCard("最小化行为", 28, 237, 574, 118);
            closeToTrayBox = MakeCheckBox("点击关闭按钮时隐藏到系统托盘", data.MinimizeToTrayOnClose, 20, 46);
            minimizeToTrayBox = MakeCheckBox("点击最小化按钮时隐藏到系统托盘", data.MinimizeToTrayOnMinimize, 20, 78);
            trayCard.Controls.Add(closeToTrayBox);
            trayCard.Controls.Add(minimizeToTrayBox);

            RoundedPanel floatingCard = BuildCard("桌面悬浮提醒", 28, 369, 574, 118);
            floatingEnabledBox = MakeCheckBox("显示可拖动的今日任务悬浮助手", data.FloatingReminderEnabled, 20, 46);
            floatingTopMostBox = MakeCheckBox("悬浮助手始终显示在其他窗口上方", data.FloatingReminderTopMost, 20, 78);
            floatingCard.Controls.Add(floatingEnabledBox);
            floatingCard.Controls.Add(floatingTopMostBox);
            floatingEnabledBox.CheckedChanged += delegate
            {
                floatingTopMostBox.Enabled = floatingEnabledBox.Checked;
            };
            floatingTopMostBox.Enabled = floatingEnabledBox.Checked;

            Label tip = Theme.MakeLabel("隐藏到托盘后，可双击右下角应用图标恢复；右键可彻底退出。",
                Theme.SmallFont, Theme.Muted);
            tip.Location = new Point(30, 508);
            Controls.Add(tip);

            Button save = Theme.MakeButton("保存设置", true);
            save.Size = new Size(118, 40);
            save.Location = new Point(484, 522);
            save.Click += delegate { DialogResult = DialogResult.OK; Close(); };
            Controls.Add(save);
            AcceptButton = save;

            Button cancel = Theme.MakeButton("取消", false);
            cancel.Size = new Size(86, 40);
            cancel.Location = new Point(388, 522);
            cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(cancel);
            CancelButton = cancel;
        }

        private RoundedPanel BuildCard(string title, int x, int y, int width, int height)
        {
            RoundedPanel card = new RoundedPanel();
            card.CornerRadius = 15;
            card.Location = new Point(x, y);
            card.Size = new Size(width, height);
            card.BackColor = Theme.Card;
            Controls.Add(card);

            Label label = Theme.MakeLabel(title, Theme.SectionFont, Theme.Text);
            label.Location = new Point(18, 14);
            card.Controls.Add(label);
            return card;
        }

        private CheckBox MakeCheckBox(string text, bool isChecked, int x, int y)
        {
            CheckBox box = new CheckBox();
            box.Text = text;
            box.Checked = isChecked;
            box.Location = new Point(x, y);
            box.Size = new Size(520, 26);
            box.Font = Theme.BodyFont;
            box.ForeColor = Theme.Text;
            box.BackColor = Color.Transparent;
            box.Cursor = Cursors.Hand;
            return box;
        }
    }
}
