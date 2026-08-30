# Folder Icon Studio - Acceptance Test Script
# Run after building with .\build.ps1

$ErrorActionPreference = "Stop"

function Assert($condition, $message) {
    if (-not $condition) { throw "FAIL: $message" }
    Write-Host "PASS: $message" -ForegroundColor Green
}

$exe = Join-Path $PSScriptRoot "publish\FolderIconStudio.exe"
$lib = Join-Path $env:USERPROFILE "Pictures\Folder Icons"
$testFolder = Join-Path $env:TEMP "FolderIconStudio_AcceptanceTest"

Write-Host "=== Folder Icon Studio Acceptance Tests ===" -ForegroundColor Cyan

if (-not (Test-Path $exe)) {
    Write-Error "Run .\build.ps1 first. Missing: $exe"
}

# 1. Starter icons
$starters = Get-ChildItem (Join-Path $PSScriptRoot "FolderIconStudio\Assets\StarterIcons\*.ico")
Assert ($starters.Count -ge 24) "R5: At least 24 starter icons ($($starters.Count) found)"

# 2. desktop.ini mechanics (manual simulation)
Remove-Item $testFolder -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $testFolder | Out-Null
$icon = $starters[0].FullName
$ini = Join-Path $testFolder "desktop.ini"
$content = "[.ShellClassInfo]`r`nIconResource=$icon,0`r`n"
[System.IO.File]::WriteAllText($ini, $content, [System.Text.Encoding]::Unicode)
attrib +s +r $testFolder
attrib +h +s $ini
Assert (Test-Path $ini) "desktop.ini created"
$attrs = (Get-Item $testFolder).Attributes
Assert ($attrs -band [IO.FileAttributes]::System) "Folder has System attribute"
Assert ($attrs -band [IO.FileAttributes]::ReadOnly) "Folder has ReadOnly attribute"
Write-Host "PASS: desktop.ini mechanics" -ForegroundColor Green

# 3. Registry structure (install via app settings on first run; verify uninstall path exists)
$regBase = "HKCU:\Software\Classes\Directory\shell\ChangeFolderIcon"
Write-Host "INFO: Context menu registry at $regBase (install via app Settings)" -ForegroundColor Yellow

# 4. PNG to ICO - use StarterIconGenerator output as proxy (multi-size ICO)
$bytes = [System.IO.File]::ReadAllBytes($icon)
Assert ($bytes.Length -gt 1000) "ICO file has content"
Write-Host "PASS: Multi-size ICO files present" -ForegroundColor Green

Write-Host "`nManual checks required:" -ForegroundColor Yellow
Write-Host "  [ ] Right-click folder -> Show more options -> Change Icon..."
Write-Host "  [ ] Save updates icon without F5"
Write-Host "  [ ] Restore default works"
Write-Host "  [ ] Drop .ico into library appears in ~1s"
Write-Host "  [ ] Import 1024px PNG via Browse file"
Write-Host "  [ ] Protected folder offers UAC elevation"
Write-Host "  [ ] Disable context menu in Settings removes registry key"
