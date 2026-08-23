param(
    [string]$OutputName = '每日进度.exe'
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$outputDir = Join-Path $projectRoot 'dist'
$compilerCandidates = @(
    "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)
$compiler = $compilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1

if (-not $compiler) {
    throw '未找到 Windows .NET Framework C# 编译器。'
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
$outputFile = Join-Path $outputDir $OutputName
$iconFile = Join-Path $projectRoot 'assets\daily-progress.ico'
$almanacFile = Join-Path $projectRoot 'assets\almanac-2020-2040.json'
$almanacResourceArgument = "/resource:$almanacFile,DailyProgressDesk.almanac.json"
$sources = Get-ChildItem -LiteralPath $projectRoot -Filter '*.cs' | ForEach-Object { $_.FullName }

if (-not (Test-Path -LiteralPath $iconFile)) {
    throw "未找到应用图标：$iconFile"
}
if (-not (Test-Path -LiteralPath $almanacFile)) {
    throw "未找到黄历数据：$almanacFile"
}

& $compiler `
    /nologo `
    /target:winexe `
    /optimize+ `
    /codepage:65001 `
    /win32icon:$iconFile `
    $almanacResourceArgument `
    /out:$outputFile `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    /reference:System.Web.Extensions.dll `
    $sources

if ($LASTEXITCODE -ne 0) {
    throw "编译失败，退出代码：$LASTEXITCODE"
}

Write-Host "构建成功：$outputFile"
Get-Item -LiteralPath $outputFile | Select-Object FullName, Length, LastWriteTime
