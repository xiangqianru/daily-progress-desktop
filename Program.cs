using System;
using System.Threading;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool created;
            using (Mutex mutex = new Mutex(true, "DailyProgressDesk.SingleInstance", out created))
            {
                if (!created)
                {
                    MessageBox.Show("每日进度已经在运行。", "每日进度",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                try
                {
                    bool restart;
                    do
                    {
                        DataStore store = new DataStore();
                        Theme.ApplyPalette(store.Data.ThemeName);
                        StartupManager.RefreshEnabledPath(store.Data.StartWithWindows);
                        MainForm form = new MainForm(store);
                        Application.Run(form);
                        restart = form.RestartRequested;
                    }
                    while (restart);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("程序启动失败：\r\n" + ex.Message,
                        "每日进度", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
