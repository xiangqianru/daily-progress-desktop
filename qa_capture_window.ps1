param(
    [string]$TitleContains = ''
)

$ErrorActionPreference = 'Stop'

Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class WindowCaptureNative {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int command);
    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);
    public delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumProc callback, IntPtr lParam);
    [DllImport("user32.dll", CharSet=CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int max);
    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware();
    public static IntPtr FindVisibleWindow(string titlePart) {
        IntPtr found = IntPtr.Zero;
        EnumWindows(delegate(IntPtr hWnd, IntPtr lParam) {
            if (!IsWindowVisible(hWnd)) return true;
            var text = new System.Text.StringBuilder(512);
            GetWindowText(hWnd, text, text.Capacity);
            if (text.ToString().Contains(titlePart)) { found = hWnd; return false; }
            return true;
        }, IntPtr.Zero);
        return found;
    }
}
'@

[WindowCaptureNative]::SetProcessDPIAware() | Out-Null

$windowHandle = [IntPtr]::Zero
if ($TitleContains) {
    $windowHandle = [WindowCaptureNative]::FindVisibleWindow($TitleContains)
    if ($windowHandle -eq [IntPtr]::Zero) {
        $process = Get-Process | Where-Object { $_.MainWindowTitle -like "*$TitleContains*" } | Select-Object -First 1
        if ($process) { $windowHandle = $process.MainWindowHandle }
    }
} else {
    $process = Get-Process -Name '每日进度' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
    if ($process) { $windowHandle = $process.MainWindowHandle }
}
if ($windowHandle -eq [IntPtr]::Zero) { throw '没有找到目标窗口。' }

[WindowCaptureNative]::ShowWindow($windowHandle, 9) | Out-Null
if (-not $TitleContains) {
    [WindowCaptureNative]::MoveWindow($windowHandle, 80, 80, 1080, 760, $true) | Out-Null
}
[WindowCaptureNative]::SetForegroundWindow($windowHandle) | Out-Null
Start-Sleep -Milliseconds 600
$rect = New-Object WindowCaptureNative+RECT
if (-not [WindowCaptureNative]::GetWindowRect($windowHandle, [ref]$rect)) {
    throw '无法获取窗口位置。'
}

$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top
$bitmap = New-Object System.Drawing.Bitmap($width, $height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
try {
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bitmap.Size)
    $outputPath = Join-Path $PSScriptRoot 'qa-main-window.png'
    $bitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host $outputPath
}
finally {
    $graphics.Dispose()
    $bitmap.Dispose()
}
