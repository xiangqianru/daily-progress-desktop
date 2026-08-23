using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class DailyTaskDialog : Form
    {
        private readonly DailyTemplate template;
        private readonly TextBox titleBox;
        private readonly ComboBox repeatBox;
        private readonly CheckedListBox daysList;
        private readonly int[] dayBits = { 1, 2, 3, 4, 5, 6, 0 };

        public DailyTaskDialog(DailyTemplate template, bool isNew)
        {
            this.template = template;
            Text = isNew ? "新建每日任务" : "编辑每日任务";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(470, 430);
            BackColor = Theme.Background;
            Font = Theme.BodyFont;
            Icon = Theme.GetAppIcon();
            Theme.ApplyWindowChrome(this);
            AutoScaleMode = AutoScaleMode.Dpi;

            Label heading = Theme.MakeLabel(Text, Theme.HeaderFont, Theme.Text);
            heading.Location = new Point(24, 20);
            Controls.Add(heading);

            Label nameLabel = Theme.MakeLabel("任务名称", Theme.SmallFont, Theme.Muted);
            nameLabel.Location = new Point(26, 78);
            Controls.Add(nameLabel);

            titleBox = new TextBox();
            titleBox.Location = new Point(27, 104);
            titleBox.Size = new Size(414, 30);
            titleBox.Font = Theme.BodyFont;
            titleBox.Text = template.Title;
            Controls.Add(titleBox);

            Label repeatLabel = Theme.MakeLabel("重复周期", Theme.SmallFont, Theme.Muted);
            repeatLabel.Location = new Point(26, 153);
            Controls.Add(repeatLabel);

            repeatBox = new ComboBox();
            repeatBox.DropDownStyle = ComboBoxStyle.DropDownList;
            repeatBox.Items.AddRange(new object[] { "每天", "工作日", "自定义星期" });
            repeatBox.Location = new Point(27, 179);
            repeatBox.Size = new Size(414, 30);
            repeatBox.Font = Theme.BodyFont;
            repeatBox.SelectedIndex = template.RepeatMode == "Weekdays" ? 1 :
                (template.RepeatMode == "Custom" ? 2 : 0);
            Controls.Add(repeatBox);

            daysList = new CheckedListBox();
            daysList.Items.AddRange(new object[] { "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日" });
            daysList.Location = new Point(27, 225);
            daysList.Size = new Size(414, 112);
            daysList.MultiColumn = true;
            daysList.ColumnWidth = 125;
            daysList.CheckOnClick = true;
            daysList.Font = Theme.SmallFont;
            for (int i = 0; i < dayBits.Length; i++)
                daysList.SetItemChecked(i, (template.DaysMask & (1 << dayBits[i])) != 0);
            Controls.Add(daysList);

            repeatBox.SelectedIndexChanged += delegate { daysList.Enabled = repeatBox.SelectedIndex == 2; };
            daysList.Enabled = repeatBox.SelectedIndex == 2;

            Button save = Theme.MakeButton("保存", true);
            save.Location = new Point(331, 366);
            save.Size = new Size(110, 40);
            save.Click += SaveClicked;
            Controls.Add(save);
            AcceptButton = save;

            Button cancel = Theme.MakeButton("取消", false);
            cancel.Location = new Point(215, 366);
            cancel.Size = new Size(105, 40);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);
            CancelButton = cancel;

            if (!isNew)
            {
                Button delete = Theme.MakeButton("删除任务", false);
                delete.ForeColor = Theme.Danger;
                delete.Location = new Point(27, 366);
                delete.Size = new Size(105, 40);
                delete.Click += delegate { DialogResult = DialogResult.Abort; Close(); };
                Controls.Add(delete);
            }
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            string title = titleBox.Text.Trim();
            if (title.Length == 0)
            {
                MessageBox.Show("请输入任务名称。", "每日任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
                titleBox.Focus();
                return;
            }

            int mask = 0;
            for (int i = 0; i < dayBits.Length; i++)
                if (daysList.GetItemChecked(i)) mask |= 1 << dayBits[i];
            if (repeatBox.SelectedIndex == 2 && mask == 0)
            {
                MessageBox.Show("请至少选择一天。", "每日任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            template.Title = title;
            template.RepeatMode = repeatBox.SelectedIndex == 1 ? "Weekdays" :
                (repeatBox.SelectedIndex == 2 ? "Custom" : "Daily");
            template.DaysMask = repeatBox.SelectedIndex == 0 ? 127 :
                (repeatBox.SelectedIndex == 1 ? 62 : mask);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public class ProjectEditDialog : Form
    {
        private readonly ProjectTask project;
        private readonly bool isNew;
        private readonly TextBox titleBox;
        private readonly CheckBox dueEnabled;
        private readonly DateTimePicker duePicker;
        private readonly ComboBox priorityBox;
        private readonly TextBox notesBox;
        private readonly TextBox stepsBox;

        public ProjectEditDialog(ProjectTask project, bool isNew)
        {
            this.project = project;
            this.isNew = isNew;
            stepsBox = null;
            Text = isNew ? "新建完成型任务" : "编辑任务信息";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, isNew ? 720 : 550);
            BackColor = Theme.Background;
            Font = Theme.BodyFont;
            Icon = Theme.GetAppIcon();
            Theme.ApplyWindowChrome(this);
            AutoScaleMode = AutoScaleMode.Dpi;

            Label heading = Theme.MakeLabel(Text, Theme.HeaderFont, Theme.Text);
            heading.Location = new Point(26, 20);
            Controls.Add(heading);

            AddCaption("任务名称", 80);
            titleBox = new TextBox();
            titleBox.Location = new Point(28, 106);
            titleBox.Size = new Size(464, 30);
            titleBox.Font = Theme.BodyFont;
            titleBox.Text = project.Title;
            Controls.Add(titleBox);

            AddCaption("优先级", 154);
            priorityBox = new ComboBox();
            priorityBox.DropDownStyle = ComboBoxStyle.DropDownList;
            priorityBox.Items.AddRange(new object[] { "普通", "重要", "紧急" });
            priorityBox.Location = new Point(28, 180);
            priorityBox.Size = new Size(210, 30);
            priorityBox.SelectedItem = project.Priority;
            if (priorityBox.SelectedIndex < 0) priorityBox.SelectedIndex = 0;
            Controls.Add(priorityBox);

            dueEnabled = new CheckBox();
            dueEnabled.Text = "设置截止日期";
            dueEnabled.Location = new Point(278, 154);
            dueEnabled.AutoSize = true;
            DateTime parsed;
            dueEnabled.Checked = DateTime.TryParse(project.DueDate, out parsed);
            Controls.Add(dueEnabled);

            duePicker = new DateTimePicker();
            duePicker.Format = DateTimePickerFormat.Custom;
            duePicker.CustomFormat = "yyyy年M月d日";
            duePicker.Location = new Point(278, 180);
            duePicker.Size = new Size(214, 30);
            duePicker.Value = dueEnabled.Checked ? parsed : DateTime.Today.AddDays(14);
            duePicker.Enabled = dueEnabled.Checked;
            Controls.Add(duePicker);
            dueEnabled.CheckedChanged += delegate { duePicker.Enabled = dueEnabled.Checked; };

            AddCaption("备注", 230);
            notesBox = new TextBox();
            notesBox.Location = new Point(28, 256);
            notesBox.Size = new Size(464, isNew ? 100 : 205);
            notesBox.Multiline = true;
            notesBox.ScrollBars = ScrollBars.Vertical;
            notesBox.Font = Theme.BodyFont;
            notesBox.Text = project.Notes;
            Controls.Add(notesBox);

            if (isNew)
            {
                AddCaption("任务步骤（每行一个，至少填写一步）", 376);
                stepsBox = new TextBox();
                stepsBox.Location = new Point(28, 402);
                stepsBox.Size = new Size(464, 205);
                stepsBox.Multiline = true;
                stepsBox.AcceptsReturn = true;
                stepsBox.ScrollBars = ScrollBars.Vertical;
                stepsBox.Font = Theme.BodyFont;
                List<string> existing = new List<string>();
                foreach (ProjectStep step in project.Steps) existing.Add(step.Title);
                stepsBox.Lines = existing.ToArray();
                Controls.Add(stepsBox);

                Label stepTip = Theme.MakeLabel(
                    "每完成一步，系统会按步骤总数自动计算精确进度。",
                    Theme.SmallFont, Theme.Muted);
                stepTip.Location = new Point(29, 616);
                Controls.Add(stepTip);
            }

            int buttonY = isNew ? 656 : 487;
            Button save = Theme.MakeButton(isNew ? "创建任务" : "保存", true);
            save.Location = new Point(322, buttonY);
            save.Size = new Size(170, 40);
            save.Click += SaveClicked;
            Controls.Add(save);
            AcceptButton = save;

            Button cancel = Theme.MakeButton("取消", false);
            cancel.Location = new Point(207, buttonY);
            cancel.Size = new Size(105, 40);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);
            CancelButton = cancel;
        }

        private void AddCaption(string text, int y)
        {
            Label caption = Theme.MakeLabel(text, Theme.SmallFont, Theme.Muted);
            caption.Location = new Point(28, y);
            Controls.Add(caption);
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            string title = titleBox.Text.Trim();
            if (title.Length == 0)
            {
                MessageBox.Show("请输入任务名称。", "完成型任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
                titleBox.Focus();
                return;
            }
            project.Title = title;
            project.Priority = priorityBox.SelectedItem.ToString();
            project.DueDate = dueEnabled.Checked ? duePicker.Value.Date.ToString("yyyy-MM-dd") : "";
            project.Notes = notesBox.Text.Trim();

            if (isNew)
            {
                List<string> stepTitles = new List<string>();
                foreach (string line in stepsBox.Lines)
                {
                    string stepTitle = line.Trim();
                    if (stepTitle.Length > 0) stepTitles.Add(stepTitle);
                }
                if (stepTitles.Count == 0)
                {
                    MessageBox.Show("请至少填写一个任务步骤，每行填写一步。", "完成型任务",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    stepsBox.Focus();
                    return;
                }

                project.Steps.Clear();
                foreach (string stepTitle in stepTitles)
                    project.Steps.Add(new ProjectStep { Title = stepTitle });
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public class TextPromptDialog : Form
    {
        private readonly TextBox input;
        public string Value { get; private set; }

        public TextPromptDialog(string title, string initial)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(450, 170);
            BackColor = Theme.Background;
            Font = Theme.BodyFont;
            Icon = Theme.GetAppIcon();
            Theme.ApplyWindowChrome(this);
            AutoScaleMode = AutoScaleMode.Dpi;

            Label label = Theme.MakeLabel(title, Theme.SectionFont, Theme.Text);
            label.Location = new Point(22, 18);
            Controls.Add(label);

            input = new TextBox();
            input.Location = new Point(24, 58);
            input.Size = new Size(402, 30);
            input.Font = Theme.BodyFont;
            input.Text = initial;
            input.SelectAll();
            Controls.Add(input);

            Button ok = Theme.MakeButton("确定", true);
            ok.Location = new Point(326, 110);
            ok.Size = new Size(100, 38);
            ok.Click += delegate
            {
                Value = input.Text.Trim();
                if (Value.Length == 0)
                {
                    MessageBox.Show("内容不能为空。", title);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(ok);
            AcceptButton = ok;

            Button cancel = Theme.MakeButton("取消", false);
            cancel.Location = new Point(216, 110);
            cancel.Size = new Size(100, 38);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);
            CancelButton = cancel;
        }
    }
}
