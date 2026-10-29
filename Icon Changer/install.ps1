#Requires -Version 5.1
<#
.SYNOPSIS
  Optional installer for Folder Icon Studio (portable app + shortcuts).

.DESCRIPTION
  Copies FolderIconStudio.exe to %LOCALAPPDATA%\FolderIconStudio
  and creates Start Menu + Desktop shortcuts. No admin rights required.
#>
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceExe = Join-Path $root "publish\FolderIconStudio.exe"
if (-not (Test-Path $sourceExe)) {
    $sourceExe = Join-Path $root "FolderIconStudio.exe"
}
if (-not (Test-Path $sourceExe)) {
    Write-Error "FolderIconStudio.exe not found next to this script or in publish\"
}

$installDir = Join-Path $env:LOCALAPPDATA "FolderIconStudio"
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
$destExe = Join-Path $installDir "FolderIconStudio.exe"
Copy-Item $sourceExe $destExe -Force

$wsh = New-Object -ComObject WScript.Shell
$desktop = [Environment]::GetFolderPath("Desktop")
$startMenu = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"

foreach ($target in @($desktop, $startMenu)) {
    $lnk = Join-Path $target "Folder Icon Studio.lnk"
    $sc = $wsh.CreateShortcut($lnk)
    $sc.TargetPath = $destExe
    $sc.WorkingDirectory = $installDir
    $sc.Description = "Change Windows folder icons"
    $sc.Save()
}

Write-Host "Installed to: $installDir" -ForegroundColor Green
Write-Host "Shortcuts created on Desktop and Start Menu." -ForegroundColor Green
Write-Host "Launch Folder Icon Studio from either shortcut — no Python required." -ForegroundColor Cyan
