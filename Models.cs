using System;
using System.Collections.Generic;

namespace DailyProgressDesk
{
    public class AppData
    {
        public int Version { get; set; }
        public string ThemeName { get; set; }
        public bool StartWithWindows { get; set; }
        public bool StartMinimized { get; set; }
        public bool MinimizeToTrayOnClose { get; set; }
        public bool MinimizeToTrayOnMinimize { get; set; }
        public bool FloatingReminderEnabled { get; set; }
        public bool FloatingReminderTopMost { get; set; }
        public int FloatingReminderX { get; set; }
        public int FloatingReminderY { get; set; }
        public List<DailyTemplate> DailyTemplates { get; set; }
        public List<DailyDay> DailyDays { get; set; }
        public List<ProjectTask> Projects { get; set; }

        public AppData()
        {
            Version = 3;
            ThemeName = "Blue";
            StartWithWindows = false;
            StartMinimized = false;
            MinimizeToTrayOnClose = true;
            MinimizeToTrayOnMinimize = true;
            FloatingReminderEnabled = true;
            FloatingReminderTopMost = true;
            FloatingReminderX = -1;
            FloatingReminderY = -1;
            DailyTemplates = new List<DailyTemplate>();
            DailyDays = new List<DailyDay>();
            Projects = new List<ProjectTask>();
        }
    }

    public class DailyTemplate
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string RepeatMode { get; set; }
        public int DaysMask { get; set; }
        public int SortOrder { get; set; }
        public bool IsArchived { get; set; }

        public DailyTemplate()
        {
            Id = Guid.NewGuid().ToString("N");
            Title = "";
            RepeatMode = "Daily";
            DaysMask = 127;
        }

        public bool RunsOn(DateTime date)
        {
            if (IsArchived) return false;
            if (RepeatMode == "Daily") return true;
            if (RepeatMode == "Weekdays")
                return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;

            int bit = 1 << (int)date.DayOfWeek;
            return (DaysMask & bit) != 0;
        }

        public string RepeatText()
        {
            if (RepeatMode == "Daily") return "每天";
            if (RepeatMode == "Weekdays") return "工作日";

            string[] names = { "日", "一", "二", "三", "四", "五", "六" };
            List<string> selected = new List<string>();
            for (int i = 0; i < 7; i++)
                if ((DaysMask & (1 << i)) != 0) selected.Add("周" + names[i]);
            return selected.Count == 0 ? "未设置" : string.Join("、", selected.ToArray());
        }
    }

    public class DailyDay
    {
        public string Date { get; set; }
        public List<DailyDayItem> Items { get; set; }

        public DailyDay()
        {
            Date = "";
            Items = new List<DailyDayItem>();
        }
    }

    public class DailyDayItem
    {
        public string TemplateId { get; set; }
        public string Title { get; set; }
        public bool IsDone { get; set; }
        public string CompletedAt { get; set; }

        public DailyDayItem()
        {
            TemplateId = "";
            Title = "";
            CompletedAt = "";
        }
    }

    public class ProjectTask
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string DueDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string CreatedAt { get; set; }
        public string CompletedAt { get; set; }
        public int SortOrder { get; set; }
        public List<ProjectStep> Steps { get; set; }

        public ProjectTask()
        {
            Id = Guid.NewGuid().ToString("N");
            Title = "";
            DueDate = "";
            Priority = "普通";
            Status = "进行中";
            Notes = "";
            CreatedAt = DateTime.Now.ToString("s");
            CompletedAt = "";
            Steps = new List<ProjectStep>();
        }

        public int DoneCount()
        {
            int count = 0;
            foreach (ProjectStep step in Steps) if (step.IsDone) count++;
            return count;
        }

        public int ProgressPercent()
        {
            if (Steps.Count == 0) return 0;
            return (int)Math.Round(DoneCount() * 100.0 / Steps.Count,
                MidpointRounding.AwayFromZero);
        }

        public double ProgressValue()
        {
            if (Steps.Count == 0) return 0.0;
            return DoneCount() * 100.0 / Steps.Count;
        }

        public string ProgressText()
        {
            double value = ProgressValue();
            string percent = Math.Abs(value - Math.Round(value)) < 0.0001
                ? value.ToString("0")
                : value.ToString("0.0");
            return DoneCount() + " / " + Steps.Count + " · " + percent + "%";
        }

        public string NextStep()
        {
            foreach (ProjectStep step in Steps)
                if (!step.IsDone) return step.Title;
            return Steps.Count == 0 ? "尚未添加步骤" : "所有步骤已完成";
        }
    }

    public class ProjectStep
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool IsDone { get; set; }
        public string CompletedAt { get; set; }

        public ProjectStep()
        {
            Id = Guid.NewGuid().ToString("N");
            Title = "";
            CompletedAt = "";
        }
    }
}
