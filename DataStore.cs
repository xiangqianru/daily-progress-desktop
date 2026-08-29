using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace DailyProgressDesk
{
    public class DataStore
    {
        private readonly string dataDirectory;
        private readonly string dataFile;
        private readonly JavaScriptSerializer serializer;

        public AppData Data { get; private set; }
        public string DataFile { get { return dataFile; } }

        public DataStore() : this(null)
        {
        }

        internal DataStore(string dataDirectoryOverride)
        {
            dataDirectory = string.IsNullOrWhiteSpace(dataDirectoryOverride)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DailyProgressDesk")
                : dataDirectoryOverride;
            dataFile = Path.Combine(dataDirectory, "tasks.json");
            serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = 8 * 1024 * 1024;
            Data = Load();
            Normalize();
            if (Data.DailyTemplates.Count == 0 && Data.DailyDays.Count == 0 && Data.Projects.Count == 0)
                SeedExamples();
            EnsureDay(DateTime.Today);
            Save();
        }

        private AppData Load()
        {
            try
            {
                if (!File.Exists(dataFile)) return new AppData();
                string json = File.ReadAllText(dataFile, Encoding.UTF8);
                AppData loaded = serializer.Deserialize<AppData>(json);
                return loaded ?? new AppData();
            }
            catch
            {
                try
                {
                    if (File.Exists(dataFile))
                    {
                        Directory.CreateDirectory(dataDirectory);
                        File.Copy(dataFile, Path.Combine(dataDirectory,
                            "tasks-recovery-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json"), true);
                    }
                }
                catch { }
                return new AppData();
            }
        }

        private void Normalize()
        {
            if (Data.Version < 2)
            {
                Data.MinimizeToTrayOnClose = true;
                Data.MinimizeToTrayOnMinimize = true;
                Data.FloatingReminderEnabled = true;
                Data.FloatingReminderTopMost = true;
                Data.FloatingReminderX = -1;
                Data.FloatingReminderY = -1;
                Data.Version = 2;
            }
            if (string.IsNullOrWhiteSpace(Data.ThemeName)) Data.ThemeName = "Blue";
            if (Data.DailyTemplates == null) Data.DailyTemplates = new List<DailyTemplate>();
            if (Data.DailyDays == null) Data.DailyDays = new List<DailyDay>();
            if (Data.Projects == null) Data.Projects = new List<ProjectTask>();
            if (Data.Version < 3)
            {
                ProjectOrdering.InitializeLegacyOrder(Data.Projects);
                Data.Version = 3;
            }
            foreach (DailyDay day in Data.DailyDays)
                if (day.Items == null) day.Items = new List<DailyDayItem>();
            foreach (ProjectTask project in Data.Projects)
                if (project.Steps == null) project.Steps = new List<ProjectStep>();
        }

        private void SeedExamples()
        {
            Data.DailyTemplates.Add(new DailyTemplate { Title = "运动 30 分钟", SortOrder = 0 });
            Data.DailyTemplates.Add(new DailyTemplate { Title = "喝水 2000 ml", SortOrder = 1 });

            ProjectTask example = new ProjectTask
            {
                Title = "示例：专利申请",
                Priority = "重要",
                Status = "进行中",
                DueDate = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd"),
                Notes = "这是示例任务，可以在详情中编辑或删除。"
            };
            example.Steps.Add(new ProjectStep { Title = "整理技术方案" });
            example.Steps.Add(new ProjectStep { Title = "检索现有专利" });
            example.Steps.Add(new ProjectStep { Title = "撰写申请材料" });
            example.Steps.Add(new ProjectStep { Title = "检查并提交" });
            Data.Projects.Add(example);
        }

        public DailyDay EnsureDay(DateTime date)
        {
            string key = date.ToString("yyyy-MM-dd");
            DailyDay day = Data.DailyDays.FirstOrDefault(d => d.Date == key);
            if (day != null) return day;

            day = new DailyDay { Date = key };
            foreach (DailyTemplate template in Data.DailyTemplates.OrderBy(t => t.SortOrder))
            {
                if (!template.RunsOn(date)) continue;
                day.Items.Add(new DailyDayItem
                {
                    TemplateId = template.Id,
                    Title = template.Title,
                    IsDone = false,
                    CompletedAt = ""
                });
            }
            Data.DailyDays.Add(day);
            return day;
        }

        public void SyncTodayWithTemplates()
        {
            DailyDay today = EnsureDay(DateTime.Today);
            foreach (DailyTemplate template in Data.DailyTemplates.OrderBy(t => t.SortOrder))
            {
                if (!template.RunsOn(DateTime.Today)) continue;
                if (!today.Items.Any(i => i.TemplateId == template.Id))
                {
                    today.Items.Add(new DailyDayItem
                    {
                        TemplateId = template.Id,
                        Title = template.Title,
                        IsDone = false,
                        CompletedAt = ""
                    });
                }
                else
                {
                    DailyDayItem item = today.Items.First(i => i.TemplateId == template.Id);
                    item.Title = template.Title;
                }
            }

            today.Items.RemoveAll(i =>
            {
                DailyTemplate t = Data.DailyTemplates.FirstOrDefault(x => x.Id == i.TemplateId);
                return t == null || !t.RunsOn(DateTime.Today);
            });
            Save();
        }

        public void Save()
        {
            Directory.CreateDirectory(dataDirectory);
            string json = serializer.Serialize(Data);
            string temp = dataFile + ".tmp";
            File.WriteAllText(temp, json, new UTF8Encoding(false));

            if (File.Exists(dataFile))
            {
                string backup = dataFile + ".bak";
                File.Copy(dataFile, backup, true);
                File.Delete(dataFile);
            }
            File.Move(temp, dataFile);
        }
    }
}
