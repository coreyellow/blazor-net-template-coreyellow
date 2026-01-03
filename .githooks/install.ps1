# PowerShell script to install pre-commit hook
$hookSource = ".githooks\pre-commit"
$hookDest = ".git\hooks\pre-commit"

if (Test-Path $hookSource) {
    Copy-Item $hookSource $hookDest -Force
    Write-Host "Pre-commit hook installed successfully."
}
else {
    Write-Host "Hook source not found: $hookSource"
}
