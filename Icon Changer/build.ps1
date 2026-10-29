#Requires -Version 5.1
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Removing old context-menu integration..." -ForegroundColor Yellow
& (Join-Path $root "uninstall-context-menu.ps1")

Write-Host "Installing Python dependencies..." -ForegroundColor Cyan
pip install -r (Join-Path $root "requirements.txt")

Write-Host "Generating starter icons..." -ForegroundColor Cyan
Push-Location $root
python (Join-Path $root "folder_icon_studio\generate_starters.py")

Write-Host "Preparing bundled icons from New Icons..." -ForegroundColor Cyan
python -m folder_icon_studio.prepare_bundled
Pop-Location

$entry = Join-Path $root "main.py"
$assets = Join-Path $root "folder_icon_studio\assets"
$appIcon = Join-Path $assets "app_icon.ico"
$out = Join-Path $root "publish"

Write-Host "Building FolderIconStudio.exe..." -ForegroundColor Cyan
python -m PyInstaller --noconfirm --onefile --windowed `
    --name FolderIconStudio `
    --icon $appIcon `
    --version-file (Join-Path $root "version_info.txt") `
    --distpath $out `
    --workpath (Join-Path $root "build") `
    --specpath (Join-Path $root "build") `
    --add-data "$assets;folder_icon_studio/assets" `
    $entry

$exe = Join-Path $out "FolderIconStudio.exe"
if (Test-Path $exe) {
    Write-Host "`nDone: $exe" -ForegroundColor Green
} else {
    Write-Error "Build failed."
}
