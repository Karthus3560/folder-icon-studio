# Removes Folder Icon Studio context-menu registry keys (from the old C# build)
$keys = @(
    "HKCU:\Software\Classes\Directory\shell\ChangeFolderIcon\command",
    "HKCU:\Software\Classes\Directory\shell\ChangeFolderIcon"
)
foreach ($key in $keys) {
    if (Test-Path $key) {
        Remove-Item $key -Recurse -Force
        Write-Host "Removed: $key" -ForegroundColor Green
    }
}
Write-Host "Context menu uninstalled. You can delete publish\FolderIconStudio.exe if you no longer need it."
