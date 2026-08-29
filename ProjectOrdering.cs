using System;
using System.Collections.Generic;
using System.Linq;

namespace DailyProgressDesk
{
    internal static class ProjectOrdering
    {
        public static ProjectTask[] Active(IEnumerable<ProjectTask> projects)
        {
            return projects
                .Where(project => project.Status != "已完成")
                .OrderBy(project => project.SortOrder)
                .ThenBy(project => project.CreatedAt)
                .ToArray();
        }

        public static int Next(IEnumerable<ProjectTask> projects)
        {
            return projects.Any() ? projects.Max(project => project.SortOrder) + 1 : 0;
        }

        public static bool Move(List<ProjectTask> projects, ProjectTask project, int direction)
        {
            ProjectTask[] active = Active(projects);
            int currentIndex = Array.IndexOf(active, project);
            int targetIndex = currentIndex + direction;
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= active.Length) return false;

            ProjectTask target = active[targetIndex];
            int targetOrder = target.SortOrder;
            target.SortOrder = project.SortOrder;
            project.SortOrder = targetOrder;
            return true;
        }

        public static void InitializeLegacyOrder(List<ProjectTask> projects)
        {
            ProjectTask[] active = projects
                .Where(project => project.Status != "已完成")
                .OrderBy(project => project.Status == "等待中" ? 1 : 0)
                .ThenBy(project => DueSort(project.DueDate))
                .ToArray();
            ProjectTask[] completed = projects
                .Where(project => project.Status == "已完成")
                .ToArray();

            int order = 0;
            foreach (ProjectTask project in active) project.SortOrder = order++;
            foreach (ProjectTask project in completed) project.SortOrder = order++;
        }

        private static DateTime DueSort(string value)
        {
            DateTime result;
            return DateTime.TryParse(value, out result) ? result : DateTime.MaxValue;
        }
    }
}
