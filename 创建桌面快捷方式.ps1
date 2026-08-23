param(
    [string]$ExePathOverride = ''
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = if ($ExePathOverride) { $ExePathOverride } else { Join-Path $projectRoot 'dist\每日进度.exe' }
$exePath = [System.IO.Path]::GetFullPath($exePath)
if (-not (Test-Path -LiteralPath $exePath)) {
    throw '没有找到 dist\每日进度.exe，请先运行 build.ps1。'
}

$desktop = [Environment]::GetFolderPath('Desktop')
$shortcutPath = Join-Path $desktop '每日进度.lnk'
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = Split-Path -Parent $exePath
$shortcut.Description = '每日必做与完成型任务'
$shortcut.IconLocation = "$exePath,0"
$shortcut.Save()

Write-Host "桌面快捷方式已创建：$shortcutPath"
