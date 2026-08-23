using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class HistoryForm : Form
    {
        private readonly DataStore store;
        private readonly DataGridView dailyGrid;
        private readonly DataGridView projectGrid;

        public HistoryForm(DataStore store)
        {
            this.store = store;
            Text = "历史记录";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(720, 520);
            Size = new Size(820, 620);
            BackColor = Theme.Background;
            Font = Theme.BodyFont;
            Icon = Theme.GetAppIcon();
            Theme.ApplyWindowChrome(this);
            AutoScaleMode = AutoScaleMode.Dpi;

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 92;
            header.BackColor = Theme.Card;
            header.Padding = new Padding(26, 18, 26, 12);
            Label title = Theme.MakeLabel("历史记录", Theme.HeaderFont, Theme.Text);
            title.Location = new Point(26, 18);
            header.Controls.Add(title);
            Label tip = Theme.MakeLabel("查看每日完成情况，以及已经归档的长期任务", Theme.SmallFont, Theme.Muted);
            tip.Location = new Point(28, 58);
            header.Controls.Add(tip);
            Controls.Add(header);

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Padding = new Point(16, 8);
            tabs.Font = Theme.BodyFont;

            TabPage dailyPage = new TabPage("每日记录");
            dailyPage.BackColor = Theme.Card;
            dailyPage.Padding = new Padding(14);
            dailyGrid = MakeGrid();
            dailyGrid.Columns.Add("Date", "日期");
            dailyGrid.Columns.Add("Progress", "完成进度");
            dailyGrid.Columns.Add("Status", "结果");
            dailyGrid.Columns.Add("Items", "完成内容");
            dailyGrid.Columns[0].Width = 130;
            dailyGrid.Columns[1].Width = 110;
            dailyGrid.Columns[2].Width = 110;
            dailyGrid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dailyPage.Controls.Add(dailyGrid);
            tabs.TabPages.Add(dailyPage);

            TabPage projectPage = new TabPage("已完成任务");
            projectPage.BackColor = Theme.Card;
            projectPage.Padding = new Padding(14);
            projectGrid = MakeGrid();
            projectGrid.Columns.Add("Title", "任务");
            projectGrid.Columns.Add("Completed", "完成时间");
            projectGrid.Columns.Add("Priority", "优先级");
            projectGrid.Columns.Add("Progress", "步骤");
            projectGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            projectGrid.Columns[1].Width = 150;
            projectGrid.Columns[2].Width = 100;
            projectGrid.Columns[3].Width = 100;
            projectPage.Controls.Add(projectGrid);

            Panel projectActions = new Panel();
            projectActions.Dock = DockStyle.Bottom;
            projectActions.Height = 54;
            projectActions.Padding = new Padding(0, 8, 0, 0);
            projectActions.BackColor = Theme.Card;
            Button reopen = Theme.MakeButton("重新打开所选任务", false);
            reopen.Width = 160;
            reopen.Dock = DockStyle.Right;
            reopen.Click += ReopenProject;
            projectActions.Controls.Add(reopen);
            projectPage.Controls.Add(projectActions);
            projectActions.BringToFront();
            tabs.TabPages.Add(projectPage);

            Controls.Add(tabs);
            tabs.BringToFront();

            LoadRows();
        }

        private DataGridView MakeGrid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.BackgroundColor = Theme.Card;
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AutoGenerateColumns = false;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.PrimaryLight;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Text;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(Theme.BodyFont, FontStyle.Bold);
            grid.ColumnHeadersHeight = 40;
            grid.RowTemplate.Height = 42;
            grid.DefaultCellStyle.SelectionBackColor = Theme.PrimaryLight;
            grid.DefaultCellStyle.SelectionForeColor = Theme.Text;
            grid.DefaultCellStyle.Font = Theme.SmallFont;
            return grid;
        }

        private void LoadRows()
        {
            dailyGrid.Rows.Clear();
            foreach (DailyDay day in store.Data.DailyDays.OrderByDescending(d => d.Date).Take(120))
            {
                int total = day.Items.Count;
                int done = day.Items.Count(i => i.IsDone);
                string status = total == 0 ? "无任务" : (done == total ? "全部完成" : "未全部完成");
                string items = string.Join("、", day.Items.Where(i => i.IsDone).Select(i => i.Title).ToArray());
                int row = dailyGrid.Rows.Add(day.Date, done + " / " + total, status, items);
                if (total > 0 && done == total)
                    dailyGrid.Rows[row].Cells[2].Style.ForeColor = Theme.Success;
                else if (total > 0)
                    dailyGrid.Rows[row].Cells[2].Style.ForeColor = Theme.Warning;
            }

            projectGrid.Rows.Clear();
            foreach (ProjectTask project in store.Data.Projects
                .Where(p => p.Status == "已完成")
                .OrderByDescending(p => p.CompletedAt))
            {
                string completed = project.CompletedAt;
                DateTime date;
                if (DateTime.TryParse(completed, out date)) completed = date.ToString("yyyy-MM-dd HH:mm");
                int row = projectGrid.Rows.Add(project.Title, completed, project.Priority,
                    project.DoneCount() + " / " + project.Steps.Count);
                projectGrid.Rows[row].Tag = project.Id;
            }
        }

        private void ReopenProject(object sender, EventArgs e)
        {
            if (projectGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择一个已完成任务。", "重新打开");
                return;
            }
            string id = projectGrid.SelectedRows[0].Tag as string;
            ProjectTask project = store.Data.Projects.FirstOrDefault(p => p.Id == id);
            if (project == null) return;
            if (MessageBox.Show("将“" + project.Title + "”恢复为进行中？", "重新打开",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            project.Status = "进行中";
            project.CompletedAt = "";
            store.Save();
            LoadRows();
        }
    }
}
