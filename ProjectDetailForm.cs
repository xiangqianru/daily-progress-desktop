using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class ProjectDetailForm : Form
    {
        private readonly DataStore store;
        private readonly ProjectTask project;
        private readonly Label titleLabel;
        private readonly Label metaLabel;
        private readonly Label progressLabel;
        private readonly AccentProgressBar progressBar;
        private readonly ComboBox statusBox;
        private readonly FlowLayoutPanel stepsFlow;

        public ProjectDetailForm(DataStore store, ProjectTask project)
        {
            this.store = store;
            this.project = project;
            Text = "任务详情 · " + project.Title;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(720, 600);
            Size = new Size(780, 690);
            BackColor = Theme.Background;
            Font = Theme.BodyFont;
            Icon = Theme.GetAppIcon();
            Theme.ApplyWindowChrome(this);
            AutoScaleMode = AutoScaleMode.Dpi;

            TableLayoutPanel rootLayout = new TableLayoutPanel();
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.BackColor = Theme.Background;
            rootLayout.ColumnCount = 1;
            rootLayout.RowCount = 4;
            rootLayout.Margin = new Padding(0);
            rootLayout.Padding = new Padding(0);
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 155F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
            Controls.Add(rootLayout);

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Theme.Card;
            header.Padding = new Padding(26);

            titleLabel = Theme.MakeLabel(project.Title, Theme.HeaderFont, Theme.Text);
            titleLabel.Location = new Point(26, 20);
            titleLabel.MaximumSize = new Size(520, 38);
            header.Controls.Add(titleLabel);

            metaLabel = Theme.MakeLabel("", Theme.SmallFont, Theme.Muted);
            metaLabel.Location = new Point(28, 66);
            header.Controls.Add(metaLabel);

            progressBar = new AccentProgressBar();
            progressBar.Location = new Point(28, 112);
            progressBar.Height = 10;
            progressBar.Width = 620;
            progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            header.Controls.Add(progressBar);

            progressLabel = Theme.MakeLabel("0 / 0 · 0%", Theme.SectionFont, Theme.Primary);
            progressLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressLabel.Location = new Point(680, 103);
            header.Controls.Add(progressLabel);

            Button edit = Theme.MakeButton("编辑信息", false);
            edit.Size = new Size(100, 36);
            edit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            edit.Location = new Point(642, 22);
            edit.Click += delegate
            {
                using (ProjectEditDialog dialog = new ProjectEditDialog(project, false))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        store.Save();
                        RefreshView();
                    }
                }
            };
            header.Controls.Add(edit);

            header.Resize += delegate
            {
                edit.Left = header.ClientSize.Width - 126;
                progressLabel.Left = header.ClientSize.Width - progressLabel.Width - 28;
                progressBar.Width = Math.Max(150, progressLabel.Left - 48);
                titleLabel.MaximumSize = new Size(Math.Max(250, header.ClientSize.Width - 180), 38);
            };
            rootLayout.Controls.Add(header, 0, 0);

            Panel actions = new Panel();
            actions.Dock = DockStyle.Fill;
            actions.BackColor = Theme.Background;
            actions.Padding = new Padding(26, 17, 26, 10);

            Label statusCaption = Theme.MakeLabel("状态", Theme.SmallFont, Theme.Muted);
            statusCaption.Location = new Point(27, 4);
            actions.Controls.Add(statusCaption);

            statusBox = new ComboBox();
            statusBox.DropDownStyle = ComboBoxStyle.DropDownList;
            statusBox.Items.AddRange(new object[] { "未开始", "进行中", "等待中" });
            statusBox.Location = new Point(27, 27);
            statusBox.Width = 150;
            actions.Controls.Add(statusBox);
            if (project.Status != "已完成") statusBox.SelectedItem = project.Status;
            if (statusBox.SelectedIndex < 0) statusBox.SelectedIndex = 1;
            statusBox.SelectedIndexChanged += delegate
            {
                if (project.Status == "已完成") return;
                project.Status = statusBox.SelectedItem.ToString();
                store.Save();
                RefreshView();
            };

            Button complete = Theme.MakeButton("完成并归档", true);
            complete.Size = new Size(132, 40);
            complete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            complete.Location = new Point(594, 17);
            complete.Click += delegate
            {
                int remaining = project.Steps.Count - project.DoneCount();
                if (project.Steps.Count == 0)
                {
                    MessageBox.Show("请先添加并完成任务步骤。", "完成任务",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (remaining > 0)
                {
                    MessageBox.Show("还有 " + remaining + " 个步骤未完成。全部步骤勾选后才能归档。",
                        "完成任务", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (MessageBox.Show("所有步骤均已完成，确定归档这个任务吗？", "完成任务", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;
                project.Status = "已完成";
                project.CompletedAt = DateTime.Now.ToString("s");
                store.Save();
                DialogResult = DialogResult.OK;
                Close();
            };
            actions.Controls.Add(complete);

            Button delete = Theme.MakeButton("删除", false);
            delete.ForeColor = Theme.Danger;
            delete.Size = new Size(82, 40);
            delete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            delete.Location = new Point(502, 17);
            delete.Click += delegate
            {
                if (MessageBox.Show("确定永久删除这个完成型任务吗？", "删除任务",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                store.Data.Projects.Remove(project);
                store.Save();
                DialogResult = DialogResult.Abort;
                Close();
            };
            actions.Controls.Add(delete);
            actions.Resize += delegate
            {
                complete.Left = actions.ClientSize.Width - 158;
                delete.Left = actions.ClientSize.Width - 250;
            };
            rootLayout.Controls.Add(actions, 0, 1);

            Panel addPanel = new Panel();
            addPanel.Dock = DockStyle.Fill;
            addPanel.BackColor = Theme.Card;
            addPanel.Padding = new Padding(24, 17, 24, 17);

            Button addStep = Theme.MakeButton("＋ 添加下一步", true);
            addStep.Dock = DockStyle.Right;
            addStep.Width = 150;
            addStep.Click += delegate
            {
                using (TextPromptDialog dialog = new TextPromptDialog("添加任务步骤", ""))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    project.Steps.Add(new ProjectStep { Title = dialog.Value });
                    if (project.Status == "未开始") project.Status = "进行中";
                    store.Save();
                    RefreshView();
                }
            };
            addPanel.Controls.Add(addStep);

            Label tip = Theme.MakeLabel("把大任务拆成能直接勾选的小步骤", Theme.SmallFont, Theme.Muted);
            tip.Dock = DockStyle.Left;
            tip.TextAlign = ContentAlignment.MiddleLeft;
            addPanel.Controls.Add(tip);
            rootLayout.Controls.Add(addPanel, 0, 3);

            stepsFlow = new FlowLayoutPanel();
            stepsFlow.Dock = DockStyle.Fill;
            // Keep steps in one vertical column so resize operations cannot
            // turn the hidden horizontal range into an incorrect scroll range.
            stepsFlow.FlowDirection = FlowDirection.TopDown;
            stepsFlow.WrapContents = false;
            stepsFlow.AutoScroll = true;
            stepsFlow.BackColor = Theme.Background;
            stepsFlow.Padding = new Padding(24, 12, 24, 12);
            stepsFlow.HorizontalScroll.Enabled = false;
            stepsFlow.HorizontalScroll.Visible = false;
            Theme.KeepVerticalScrollOnly(stepsFlow);
            stepsFlow.Resize += delegate
            {
                foreach (Control child in stepsFlow.Controls)
                    child.Width = GetStepWidth();
            };
            rootLayout.Controls.Add(stepsFlow, 0, 2);

            RefreshView();
        }

        private int GetStepWidth()
        {
            return Math.Max(200, stepsFlow.ClientSize.Width - stepsFlow.Padding.Horizontal
                - SystemInformation.VerticalScrollBarWidth - 4);
        }

        private void RefreshView()
        {
            titleLabel.Text = project.Title;
            Text = "任务详情 · " + project.Title;
            string due = project.DueDate.Length == 0 ? "无截止日期" : "截止 " + project.DueDate;
            metaLabel.Text = project.Priority + "优先级  ·  " + due + "  ·  " + project.Status;
            progressBar.Value = Math.Max(0, Math.Min(100, project.ProgressPercent()));
            progressLabel.Text = project.ProgressText();
            progressLabel.Left = progressLabel.Parent.ClientSize.Width - progressLabel.Width - 28;
            progressBar.Width = Math.Max(150, progressLabel.Left - 48);
            if (project.Status != "已完成") statusBox.SelectedItem = project.Status;

            stepsFlow.SuspendLayout();
            stepsFlow.Controls.Clear();
            if (project.Steps.Count == 0)
            {
                RoundedPanel empty = new RoundedPanel();
                empty.CornerRadius = 14;
                empty.Height = 100;
                empty.Width = GetStepWidth();
                empty.BackColor = Theme.Card;
                Label label = Theme.MakeLabel("还没有步骤，点击下方按钮添加第一步。", Theme.BodyFont, Theme.Muted);
                label.Location = new Point(22, 36);
                empty.Controls.Add(label);
                stepsFlow.Controls.Add(empty);
            }
            else
            {
                for (int index = 0; index < project.Steps.Count; index++)
                {
                    ProjectStep step = project.Steps[index];
                    stepsFlow.Controls.Add(BuildStepRow(step, index));
                }
            }
            stepsFlow.ResumeLayout();
        }

        private Control BuildStepRow(ProjectStep step, int index)
        {
            RoundedPanel row = new RoundedPanel();
            row.CornerRadius = 13;
            row.Height = 78;
            row.Width = GetStepWidth();
            row.BackColor = Theme.Card;
            row.Margin = new Padding(0, 0, 0, 9);

            Label number = Theme.MakeLabel((index + 1).ToString("00"), Theme.SmallFont, Theme.Muted);
            number.Location = new Point(14, 30);
            row.Controls.Add(number);

            CheckBox check = new CheckBox();
            check.Checked = step.IsDone;
            check.Location = new Point(45, 27);
            check.Size = new Size(24, 24);
            check.Cursor = Cursors.Hand;
            row.Controls.Add(check);

            Label title = Theme.MakeLabel(step.Title, Theme.BodyFont, step.IsDone ? Theme.Muted : Theme.Text);
            title.Location = new Point(78, 12);
            title.MaximumSize = new Size(Math.Max(100, row.Width - 230), 26);
            if (step.IsDone) title.Font = new Font(Theme.BodyFont, FontStyle.Strikeout);
            row.Controls.Add(title);

            double share = project.Steps.Count == 0 ? 0.0 : 100.0 / project.Steps.Count;
            string shareText = Math.Abs(share - Math.Round(share)) < 0.0001
                ? share.ToString("0") : share.ToString("0.0");
            Label contribution = Theme.MakeLabel(
                step.IsDone ? "已计入 " + shareText + "%" : "完成后计入 " + shareText + "%",
                Theme.SmallFont, step.IsDone ? Theme.Success : Theme.Muted);
            contribution.Location = new Point(79, 43);
            row.Controls.Add(contribution);

            Button edit = Theme.MakeButton("编辑", false);
            edit.Size = new Size(58, 30);
            edit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            edit.Location = new Point(row.Width - 132, 23);
            edit.Font = Theme.SmallFont;
            edit.Click += delegate
            {
                using (TextPromptDialog dialog = new TextPromptDialog("编辑任务步骤", step.Title))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    step.Title = dialog.Value;
                    store.Save();
                    RefreshView();
                }
            };
            row.Controls.Add(edit);

            Button remove = Theme.MakeButton("×", false);
            remove.ForeColor = Theme.Danger;
            remove.Size = new Size(42, 30);
            remove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            remove.Location = new Point(row.Width - 64, 23);
            remove.Click += delegate
            {
                if (MessageBox.Show("删除步骤“" + step.Title + "”？", "删除步骤",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                project.Steps.Remove(step);
                store.Save();
                RefreshView();
            };
            row.Controls.Add(remove);

            row.Resize += delegate
            {
                edit.Left = row.ClientSize.Width - 132;
                remove.Left = row.ClientSize.Width - 64;
                title.MaximumSize = new Size(Math.Max(100, row.ClientSize.Width - 230), 26);
            };

            check.CheckedChanged += delegate
            {
                step.IsDone = check.Checked;
                step.CompletedAt = check.Checked ? DateTime.Now.ToString("s") : "";
                store.Save();
                RefreshView();
            };
            return row;
        }
    }
}
