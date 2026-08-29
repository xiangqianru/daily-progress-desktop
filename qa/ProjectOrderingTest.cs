using System;
using System.Collections.Generic;

namespace DailyProgressDesk
{
    internal static class ProjectOrderingTest
    {
        private static int Main()
        {
            ProjectTask later = NewProject("较晚截止", "进行中", "2026-12-01");
            ProjectTask waiting = NewProject("等待中", "等待中", "2026-01-01");
            ProjectTask sooner = NewProject("较早截止", "进行中", "2026-09-01");
            List<ProjectTask> projects = new List<ProjectTask> { later, waiting, sooner };

            ProjectOrdering.InitializeLegacyOrder(projects);
            ProjectTask[] active = ProjectOrdering.Active(projects);
            if (active[0] != sooner || active[1] != later || active[2] != waiting)
                return Fail("legacy display order was not preserved");

            if (!ProjectOrdering.Move(projects, later, -1))
                return Fail("valid upward move was rejected");
            active = ProjectOrdering.Active(projects);
            if (active[0] != later || active[1] != sooner)
                return Fail("upward move was not persisted in sort values");

            if (ProjectOrdering.Move(projects, later, -1))
                return Fail("first task moved beyond the upper boundary");
            if (ProjectOrdering.Move(projects, waiting, 1))
                return Fail("last task moved beyond the lower boundary");
            if (ProjectOrdering.Next(projects) <= waiting.SortOrder)
                return Fail("new task order is not appended after existing tasks");

            Console.WriteLine("PASS: project manual ordering and boundaries");
            return 0;
        }

        private static ProjectTask NewProject(string title, string status, string dueDate)
        {
            return new ProjectTask
            {
                Title = title,
                Status = status,
                DueDate = dueDate,
                CreatedAt = title
            };
        }

        private static int Fail(string message)
        {
            Console.Error.WriteLine("FAIL: " + message);
            return 1;
        }
    }
}
