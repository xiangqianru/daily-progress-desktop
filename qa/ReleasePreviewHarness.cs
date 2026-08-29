using System;
using System.IO;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    internal static class ReleasePreviewHarness
    {
        [STAThread]
        private static void Main()
        {
            string previewData = Path.Combine(Path.GetTempPath(),
                "DailyProgressDesk-release-preview-v110");
            Directory.CreateDirectory(previewData);

            DataStore store = new DataStore(previewData);
            store.Data.ThemeName = "Pink";
            store.Data.StartWithWindows = false;
            store.Data.StartMinimized = false;
            store.Data.MinimizeToTrayOnClose = false;
            store.Data.MinimizeToTrayOnMinimize = false;
            store.Data.FloatingReminderEnabled = false;
            store.Data.DailyTemplates.Clear();
            store.Data.DailyDays.Clear();
            store.Data.Projects.Clear();

            store.Data.DailyTemplates.Add(new DailyTemplate
            {
                Title = "运动 30 分钟",
                SortOrder = 0
            });
            store.Data.DailyTemplates.Add(new DailyTemplate
            {
                Title = "喝水 2000 ml",
                SortOrder = 1
            });

            AddProject(store, "中期报告书", "2026-09-25", "整理技术方案", 2, 4, 0);
            AddProject(store, "专利申请材料", "2026-10-30", "核对权利要求", 1, 5, 1);
            AddProject(store, "文章修改与投稿", "2026-11-15", "完成返修", 3, 6, 2);
            AddProject(store, "实验数据整理", "", "汇总原始记录", 0, 4, 3);
            store.EnsureDay(DateTime.Today);
            store.Save();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Theme.ApplyPalette("Pink");
            Application.Run(new MainForm(store));
        }

        private static void AddProject(DataStore store, string title, string dueDate,
            string nextStep, int done, int total, int sortOrder)
        {
            ProjectTask project = new ProjectTask
            {
                Title = title,
                DueDate = dueDate,
                Status = "进行中",
                SortOrder = sortOrder
            };
            for (int index = 0; index < total; index++)
            {
                project.Steps.Add(new ProjectStep
                {
                    Title = index == done ? nextStep : "步骤 " + (index + 1),
                    IsDone = index < done,
                    CompletedAt = index < done ? DateTime.Today.ToString("s") : ""
                });
            }
            store.Data.Projects.Add(project);
        }
    }
}
