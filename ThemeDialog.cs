using System.Drawing;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class ThemeDialog : Form
    {
        public string SelectedTheme { get; private set; }

        public ThemeDialog(string currentTheme)
        {
            SelectedTheme = currentTheme;
            Text = "外观主题";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(690, 445);
            BackColor = Theme.Background;
            Font = Theme.BodyFont;
            Icon = Theme.GetAppIcon();
            Theme.ApplyWindowChrome(this);
            AutoScaleMode = AutoScaleMode.Dpi;

            Label title = Theme.MakeLabel("选择你喜欢的外观", Theme.HeaderFont, Theme.Text);
            title.Location = new Point(28, 24);
            Controls.Add(title);

            Label subtitle = Theme.MakeLabel("主题会应用到主页面、任务详情和所有操作窗口，并自动保存。",
                Theme.SmallFont, Theme.Muted);
            subtitle.Location = new Point(30, 68);
            Controls.Add(subtitle);

            FlowLayoutPanel choices = new FlowLayoutPanel();
            choices.Location = new Point(27, 108);
            choices.Size = new Size(636, 280);
            choices.FlowDirection = FlowDirection.LeftToRight;
            choices.WrapContents = false;
            choices.BackColor = Theme.Background;
            foreach (ThemePalette palette in Theme.GetPalettes())
                choices.Controls.Add(BuildChoice(palette, currentTheme));
            Controls.Add(choices);

            Label note = Theme.MakeLabel("提示：以后可随时从主页面左下角的“外观主题”切换。",
                Theme.SmallFont, Theme.Muted);
            note.Location = new Point(30, 409);
            Controls.Add(note);
        }

        private Control BuildChoice(ThemePalette palette, string currentTheme)
        {
            RoundedPanel card = new RoundedPanel();
            card.CornerRadius = 16;
            card.Size = new Size(198, 265);
            card.Margin = new Padding(5, 0, 7, 0);
            card.BackColor = palette.Card;

            RoundedPanel preview = new RoundedPanel();
            preview.CornerRadius = 12;
            preview.Location = new Point(14, 14);
            preview.Size = new Size(170, 104);
            preview.BackColor = palette.Background;
            card.Controls.Add(preview);

            RoundedPanel miniHeader = new RoundedPanel();
            miniHeader.CornerRadius = 7;
            miniHeader.Location = new Point(12, 12);
            miniHeader.Size = new Size(146, 29);
            miniHeader.BackColor = palette.Card;
            preview.Controls.Add(miniHeader);

            RoundedPanel dot = new RoundedPanel();
            dot.CornerRadius = 5;
            dot.Location = new Point(10, 9);
            dot.Size = new Size(30, 10);
            dot.BackColor = palette.Primary;
            miniHeader.Controls.Add(dot);

            RoundedPanel miniCardOne = new RoundedPanel();
            miniCardOne.CornerRadius = 6;
            miniCardOne.Location = new Point(12, 51);
            miniCardOne.Size = new Size(68, 40);
            miniCardOne.BackColor = palette.PrimaryLight;
            preview.Controls.Add(miniCardOne);

            RoundedPanel miniCardTwo = new RoundedPanel();
            miniCardTwo.CornerRadius = 6;
            miniCardTwo.Location = new Point(90, 51);
            miniCardTwo.Size = new Size(68, 40);
            miniCardTwo.BackColor = palette.SurfaceAlt;
            preview.Controls.Add(miniCardTwo);

            Label name = Theme.MakeLabel(palette.Name, Theme.SectionFont, palette.Text);
            name.Location = new Point(16, 136);
            card.Controls.Add(name);

            Label description = Theme.MakeLabel(palette.Description, Theme.SmallFont, palette.Muted);
            description.AutoSize = false;
            description.Location = new Point(17, 168);
            description.Size = new Size(166, 38);
            card.Controls.Add(description);

            RoundedButton choose = new RoundedButton();
            choose.CornerRadius = 9;
            choose.Location = new Point(16, 214);
            choose.Size = new Size(166, 36);
            choose.FlatStyle = FlatStyle.Flat;
            choose.FlatAppearance.BorderSize = 0;
            choose.BackColor = palette.Primary;
            choose.ForeColor = Color.White;
            choose.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            choose.Cursor = Cursors.Hand;
            choose.Text = string.Equals(palette.Key, currentTheme,
                System.StringComparison.OrdinalIgnoreCase) ? "正在使用" : "使用此主题";
            choose.Click += delegate
            {
                SelectedTheme = palette.Key;
                DialogResult = DialogResult.OK;
                Close();
            };
            card.Controls.Add(choose);
            return card;
        }
    }
}
