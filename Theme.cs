using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DailyProgressDesk
{
    public class ThemePalette
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Color Background { get; set; }
        public Color Card { get; set; }
        public Color SurfaceAlt { get; set; }
        public Color Text { get; set; }
        public Color Muted { get; set; }
        public Color Primary { get; set; }
        public Color PrimaryLight { get; set; }
        public Color Success { get; set; }
        public Color Warning { get; set; }
        public Color Danger { get; set; }
        public Color Border { get; set; }
    }

    public static class Theme
    {
        private const int HorizontalScrollBar = 0;

        [DllImport("user32.dll")]
        private static extern bool ShowScrollBar(IntPtr windowHandle, int bar, bool show);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute,
            ref int value, int valueSize);

        private const int DwmwaBorderColor = 34;
        private const int DwmwaCaptionColor = 35;
        private const int DwmwaTextColor = 36;

        public static string CurrentKey { get; private set; }
        public static string CurrentName { get; private set; }
        public static Color Background { get; private set; }
        public static Color Card { get; private set; }
        public static Color SurfaceAlt { get; private set; }
        public static Color Text { get; private set; }
        public static Color Muted { get; private set; }
        public static Color Primary { get; private set; }
        public static Color PrimaryLight { get; private set; }
        public static Color Success { get; private set; }
        public static Color Warning { get; private set; }
        public static Color Danger { get; private set; }
        public static Color Border { get; private set; }

        public static readonly Font HeaderFont = new Font("Microsoft YaHei UI", 20F, FontStyle.Bold);
        public static readonly Font SectionFont = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        public static readonly Font BodyFont = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular);
        public static readonly Font SmallFont = new Font("Microsoft YaHei UI", 8.8F, FontStyle.Regular);

        static Theme()
        {
            ApplyPalette("Blue");
        }

        public static ThemePalette[] GetPalettes()
        {
            return new ThemePalette[]
            {
                new ThemePalette
                {
                    Key = "Blue", Name = "海盐蓝", Description = "清爽、专注，适合日常办公",
                    Background = Color.FromArgb(244, 247, 252), Card = Color.White,
                    SurfaceAlt = Color.FromArgb(248, 250, 254), Text = Color.FromArgb(29, 43, 68),
                    Muted = Color.FromArgb(104, 121, 148), Primary = Color.FromArgb(70, 104, 232),
                    PrimaryLight = Color.FromArgb(232, 239, 255), Success = Color.FromArgb(36, 151, 105),
                    Warning = Color.FromArgb(214, 132, 41), Danger = Color.FromArgb(202, 69, 82),
                    Border = Color.FromArgb(222, 229, 241)
                },
                new ThemePalette
                {
                    Key = "Pink", Name = "樱花粉", Description = "柔和、轻盈，记录更有温度",
                    Background = Color.FromArgb(255, 247, 250), Card = Color.White,
                    SurfaceAlt = Color.FromArgb(255, 250, 252), Text = Color.FromArgb(76, 42, 58),
                    Muted = Color.FromArgb(139, 100, 118), Primary = Color.FromArgb(222, 78, 134),
                    PrimaryLight = Color.FromArgb(252, 229, 239), Success = Color.FromArgb(44, 150, 111),
                    Warning = Color.FromArgb(211, 126, 45), Danger = Color.FromArgb(196, 64, 86),
                    Border = Color.FromArgb(241, 218, 227)
                },
                new ThemePalette
                {
                    Key = "Green", Name = "森野绿", Description = "自然、舒缓，适合长期坚持",
                    Background = Color.FromArgb(243, 248, 245), Card = Color.White,
                    SurfaceAlt = Color.FromArgb(248, 252, 249), Text = Color.FromArgb(35, 65, 54),
                    Muted = Color.FromArgb(101, 127, 116), Primary = Color.FromArgb(39, 139, 99),
                    PrimaryLight = Color.FromArgb(226, 244, 236), Success = Color.FromArgb(32, 139, 88),
                    Warning = Color.FromArgb(205, 130, 42), Danger = Color.FromArgb(194, 72, 76),
                    Border = Color.FromArgb(218, 232, 224)
                }
            };
        }

        public static ThemePalette GetPalette(string key)
        {
            foreach (ThemePalette palette in GetPalettes())
                if (string.Equals(palette.Key, key, StringComparison.OrdinalIgnoreCase)) return palette;
            return GetPalettes()[0];
        }

        public static void ApplyPalette(string key)
        {
            ThemePalette palette = GetPalette(key);
            CurrentKey = palette.Key;
            CurrentName = palette.Name;
            Background = palette.Background;
            Card = palette.Card;
            SurfaceAlt = palette.SurfaceAlt;
            Text = palette.Text;
            Muted = palette.Muted;
            Primary = palette.Primary;
            PrimaryLight = palette.PrimaryLight;
            Success = palette.Success;
            Warning = palette.Warning;
            Danger = palette.Danger;
            Border = palette.Border;
        }

        public static Icon GetAppIcon()
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                return icon ?? SystemIcons.Application;
            }
            catch { return SystemIcons.Application; }
        }

        public static void ApplyWindowChrome(Form form)
        {
            if (form == null || SystemInformation.HighContrast) return;
            EventHandler apply = delegate { ApplyNativeWindowChrome(form); };
            form.HandleCreated += apply;
            form.Shown += apply;
            if (form.IsHandleCreated) ApplyNativeWindowChrome(form);
        }

        private static void ApplyNativeWindowChrome(Form form)
        {
            try
            {
                int caption = ToColorRef(PrimaryLight);
                int border = ToColorRef(Border);
                int text = ToColorRef(Text);
                DwmSetWindowAttribute(form.Handle, DwmwaCaptionColor, ref caption, sizeof(int));
                DwmSetWindowAttribute(form.Handle, DwmwaBorderColor, ref border, sizeof(int));
                DwmSetWindowAttribute(form.Handle, DwmwaTextColor, ref text, sizeof(int));
            }
            catch
            {
                // Older Windows versions keep their normal system title bar.
            }
        }

        private static int ToColorRef(Color color)
        {
            return color.R | (color.G << 8) | (color.B << 16);
        }

        public static Button MakeButton(string text, bool primary)
        {
            RoundedButton button = new RoundedButton();
            button.Text = text;
            button.AutoSize = false;
            button.Height = 38;
            button.CornerRadius = 10;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = primary ? Shift(Primary, 10) : PrimaryLight;
            button.FlatAppearance.MouseDownBackColor = primary ? Shift(Primary, -12) : Border;
            button.BackColor = primary ? Primary : SurfaceAlt;
            button.ForeColor = primary ? Color.White : Text;
            button.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            return button;
        }

        private static Color Shift(Color source, int amount)
        {
            return Color.FromArgb(source.A,
                Math.Max(0, Math.Min(255, source.R + amount)),
                Math.Max(0, Math.Min(255, source.G + amount)),
                Math.Max(0, Math.Min(255, source.B + amount)));
        }

        public static Label MakeLabel(string text, Font font, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = font;
            label.ForeColor = color;
            label.BackColor = Color.Transparent;
            label.AutoSize = true;
            return label;
        }

        public static void KeepVerticalScrollOnly(ScrollableControl control)
        {
            EventHandler hide = delegate
            {
                if (control.IsHandleCreated) ShowScrollBar(control.Handle, HorizontalScrollBar, false);
            };
            control.HandleCreated += hide;
            control.Layout += delegate
            {
                if (control.IsHandleCreated) ShowScrollBar(control.Handle, HorizontalScrollBar, false);
            };
            control.SizeChanged += hide;
        }
    }
}
