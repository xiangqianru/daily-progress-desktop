using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DailyProgressDesk.Qa
{
    internal static class FlowResizeRegressionTest
    {
        [STAThread]
        private static int Main()
        {
            Application.EnableVisualStyles();

            using (Form form = new Form())
            using (FlowLayoutPanel flow = new FlowLayoutPanel())
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-2000, -2000);
                form.Size = new Size(900, 650);

                flow.Dock = DockStyle.Fill;
                flow.FlowDirection = FlowDirection.TopDown;
                flow.WrapContents = false;
                flow.AutoScroll = true;
                flow.Padding = new Padding(0, 6, 0, 10);
                flow.HorizontalScroll.Enabled = false;
                flow.HorizontalScroll.Visible = false;
                flow.Resize += delegate { ResizeItems(flow); };

                for (int index = 0; index < 10; index++)
                {
                    Panel card = new Panel();
                    card.Height = 142;
                    card.Margin = new Padding(0, 0, 0, 10);
                    flow.Controls.Add(card);
                }

                form.Controls.Add(flow);
                form.Show();
                Application.DoEvents();

                form.Size = new Size(1250, 900);
                Application.DoEvents();
                form.Size = new Size(900, 650);
                Application.DoEvents();

                ResizeItems(flow);
                flow.PerformLayout();
                Application.DoEvents();

                int expectedWidth = GetItemWidth(flow);
                if (flow.Controls.Cast<Control>().Any(control => control.Width != expectedWidth))
                    return Fail("card width was not recalculated after resize");
                if (!flow.VerticalScroll.Visible || flow.VerticalScroll.Maximum <= flow.ClientSize.Height)
                    return Fail("vertical scroll range is missing after resize");

                flow.AutoScrollPosition = new Point(0, 300);
                Application.DoEvents();
                if (-flow.AutoScrollPosition.Y <= 0)
                    return Fail("vertical scroll position did not advance");
                if (flow.AutoScrollPosition.X != 0)
                    return Fail("unexpected horizontal scroll position");

                Console.WriteLine("PASS: vertical task scrolling survives enlarge-then-shrink");
                return 0;
            }
        }

        private static void ResizeItems(FlowLayoutPanel flow)
        {
            int width = GetItemWidth(flow);
            foreach (Control child in flow.Controls) child.Width = width;
        }

        private static int GetItemWidth(FlowLayoutPanel flow)
        {
            return Math.Max(100, flow.ClientSize.Width - flow.Padding.Horizontal
                - SystemInformation.VerticalScrollBarWidth - 4);
        }

        private static int Fail(string message)
        {
            Console.Error.WriteLine("FAIL: " + message);
            return 1;
        }
    }
}
