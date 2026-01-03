<#
.SYNOPSIS
    Script to rename the application from "blazor-net-app" to a custom name

.DESCRIPTION
    This script renames all occurrences of "blazor-net-app" and "BlazorNetApp" in the project
    to a new application name. It updates namespaces, file names, directory names,
    and configuration files.

.PARAMETER NewAppName
    The new application name in kebab-case (e.g., my-cool-app)

.EXAMPLE
    .\Rename-App.ps1 -NewAppName my-cool-app
    
.NOTES
    Author: Blazor .NET Template
    Version: 1.0
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidatePattern('^[a-z][a-z0-9-]*$')]
    [string]$NewAppName
)

# Function to convert kebab-case to PascalCase
function ConvertTo-PascalCase {
    param([string]$Text)
    
    $parts = $Text -split '-'
    $pascalCase = ($parts | ForEach-Object { 
        $_.Substring(0,1).ToUpper() + $_.Substring(1).ToLower()
    }) -join ''
    
    return $pascalCase
}

# Function to write colored output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Info {
    param([string]$Message)
    Write-ColorOutput "[INFO] $Message" "Green"
}

function Write-Warning {
    param([string]$Message)
    Write-ColorOutput "[WARNING] $Message" "Yellow"
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput "[ERROR] $Message" "Red"
}

# Main script
try {
    $NewAppNamePascal = ConvertTo-PascalCase -Text $NewAppName
    
    Write-Info "Renaming application from 'blazor-net-app' to '$NewAppName'"
    Write-Info "Pascal case name will be: $NewAppNamePascal"
    Write-Host ""
    
    # Confirm with user
    $confirmation = Read-Host "Continue with rename? (y/N)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Warning "Rename cancelled"
        exit 0
    }
    
    # Current names
    $OldAppName = "blazor-net-app"
    $OldAppNamePascal = "BlazorNetApp"
    
    Write-Info "Step 1: Renaming directories..."
    
    if (Test-Path "src\BlazorNetApp.Api") {
        Rename-Item "src\BlazorNetApp.Api" "src\$NewAppNamePascal.Api" -Force
        Write-Info "  Renamed src\BlazorNetApp.Api -> src\$NewAppNamePascal.Api"
    }
    
    if (Test-Path "tests\BlazorNetApp.Tests") {
        Rename-Item "tests\BlazorNetApp.Tests" "tests\$NewAppNamePascal.Tests" -Force
        Write-Info "  Renamed tests\BlazorNetApp.Tests -> tests\$NewAppNamePascal.Tests"
    }
    
    if (Test-Path "tests\BlazorNetApp.IntegrationTests") {
        Rename-Item "tests\BlazorNetApp.IntegrationTests" "tests\$NewAppNamePascal.IntegrationTests" -Force
        Write-Info "  Renamed tests\BlazorNetApp.IntegrationTests -> tests\$NewAppNamePascal.IntegrationTests"
    }
    
    Write-Info "Step 2: Renaming files..."
    
    if (Test-Path "src\$NewAppNamePascal.Api\BlazorNetApp.Api.csproj") {
        Rename-Item "src\$NewAppNamePascal.Api\BlazorNetApp.Api.csproj" "$NewAppNamePascal.Api.csproj" -Force
        Write-Info "  Renamed BlazorNetApp.Api.csproj -> $NewAppNamePascal.Api.csproj"
    }
    
    if (Test-Path "tests\$NewAppNamePascal.Tests\BlazorNetApp.Tests.csproj") {
        Rename-Item "tests\$NewAppNamePascal.Tests\BlazorNetApp.Tests.csproj" "$NewAppNamePascal.Tests.csproj" -Force
        Write-Info "  Renamed BlazorNetApp.Tests.csproj -> $NewAppNamePascal.Tests.csproj"
    }
    
    if (Test-Path "tests\$NewAppNamePascal.IntegrationTests\BlazorNetApp.IntegrationTests.csproj") {
        Rename-Item "tests\$NewAppNamePascal.IntegrationTests\BlazorNetApp.IntegrationTests.csproj" "$NewAppNamePascal.IntegrationTests.csproj" -Force
        Write-Info "  Renamed BlazorNetApp.IntegrationTests.csproj -> $NewAppNamePascal.IntegrationTests.csproj"
    }
    
    if (Test-Path "BlazorNetApp.sln") {
        Rename-Item "BlazorNetApp.sln" "$NewAppNamePascal.sln" -Force
        Write-Info "  Renamed BlazorNetApp.sln -> $NewAppNamePascal.sln"
    }
    
    if (Test-Path "deployment\systemd\blazor-net-app.service") {
        Rename-Item "deployment\systemd\blazor-net-app.service" "$NewAppName.service" -Force
        Write-Info "  Renamed blazor-net-app.service -> $NewAppName.service"
    }
    
    Write-Info "Step 3: Updating file contents..."
    
    # Function to update file content
    function Update-FileContent {
        param(
            [string]$FilePath,
            [string]$OldNamePascal,
            [string]$NewNamePascal,
            [string]$OldNameKebab,
            [string]$NewNameKebab
        )
        
        if (Test-Path $FilePath) {
            $content = Get-Content $FilePath -Raw -Encoding UTF8
            $content = $content -replace $OldNamePascal, $NewNamePascal
            $content = $content -replace $OldNameKebab, $NewNameKebab
            $content = $content -replace "Net App", ($NewNamePascal -replace '([A-Z])', ' $1').Trim()
            Set-Content $FilePath -Value $content -Encoding UTF8 -NoNewline
        }
    }
    
    # Update C# files
    Get-ChildItem -Path "src" -Filter "*.cs" -Recurse | ForEach-Object {
        Update-FileContent -FilePath $_.FullName -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    }
    
    Get-ChildItem -Path "tests" -Filter "*.cs" -Recurse | ForEach-Object {
        Update-FileContent -FilePath $_.FullName -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    }
    
    # Update project files
    Get-ChildItem -Path "." -Filter "*.csproj" -Recurse | ForEach-Object {
        Update-FileContent -FilePath $_.FullName -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    }
    
    # Update solution file
    if (Test-Path "$NewAppNamePascal.sln") {
        Update-FileContent -FilePath "$NewAppNamePascal.sln" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    }
    
    # Update configuration files
    $configFiles = @(
        "src/$NewAppNamePascal.Api/appsettings.json",
        "src/$NewAppNamePascal.Api/appsettings.Development.json",
        "src/$NewAppNamePascal.Api/Properties/launchSettings.json"
    )
    
    foreach ($file in $configFiles) {
        if (Test-Path $file) {
            Update-FileContent -FilePath $file -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
        }
    }
    
    # Update Razor view files
    Get-ChildItem -Path "src/$NewAppNamePascal.Api/Pages" -Filter "*.cshtml" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Update-FileContent -FilePath $_.FullName -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    }
    
    # Update Blazor component files
    Get-ChildItem -Path "src/$NewAppNamePascal.Api/Components" -Filter "*.razor" -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Update-FileContent -FilePath $_.FullName -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    }
    
    # Update VS Code configuration
    $vscodeFiles = @(
        ".vscode/settings.json",
        ".vscode/launch.json",
        ".vscode/tasks.json"
    )
    
    foreach ($file in $vscodeFiles) {
        if (Test-Path $file) {
            Update-FileContent -FilePath $file -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
        }
    }
    
    # Update deployment files
    Update-FileContent -FilePath "deployment/install.sh" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "deployment/systemd/$NewAppName.service" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    
    # Update GitHub workflows
    Update-FileContent -FilePath ".github/workflows/ci-cd.yml" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath ".github/workflows/release.yml" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    
    # Update documentation
    Update-FileContent -FilePath "README.md" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "CONTRIBUTING.md" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "docs/index.md" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    
    # Update Docker files
    Update-FileContent -FilePath "Dockerfile" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "docker-compose.yml" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    
    # Update build scripts
    Update-FileContent -FilePath "build-deb.sh" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "build-nuget.ps1" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    
    # Update docker dev scripts
    Update-FileContent -FilePath "docker-dev.sh" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "docker-dev.ps1" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    
    # Update debian package files
    Update-FileContent -FilePath "debian/DEBIAN/control.template" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "debian/DEBIAN/postinst" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "debian/DEBIAN/prerm" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    Update-FileContent -FilePath "debian/DEBIAN/postrm" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    
    # Update IMPLEMENTATION_SUMMARY.md
    Update-FileContent -FilePath "IMPLEMENTATION_SUMMARY.md" -OldNamePascal $OldAppNamePascal -NewNamePascal $NewAppNamePascal -OldNameKebab $OldAppName -NewNameKebab $NewAppName
    
    Write-Info "Step 4: Cleaning build artifacts..."
    
    Get-ChildItem -Path "src" -Include "bin","obj" -Recurse -Directory | Remove-Item -Recurse -Force
    Get-ChildItem -Path "tests" -Include "bin","obj" -Recurse -Directory | Remove-Item -Recurse -Force
    
    Write-Host ""
    Write-Info "✓ Rename complete!"
    Write-Host ""
    Write-Host "Next steps:"
    Write-Host "  1. Review the changes: git status"
    Write-Host "  2. Build the project: dotnet build"
    Write-Host "  3. Run tests: dotnet test"
    Write-Host "  4. Update README.md with your app-specific information"
    Write-Host "  5. Commit changes: git add . && git commit -m 'Rename app to $NewAppName'"
    Write-Host ""
}
catch {
    Write-Error "An error occurred: $_"
    exit 1
}
