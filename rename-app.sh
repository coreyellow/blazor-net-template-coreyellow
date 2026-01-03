#!/bin/bash
set -e

# Script to rename the application from "blazor-net-app" to a custom name
# Usage: ./rename-app.sh <new-app-name>

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored messages
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if new name is provided
if [ $# -eq 0 ]; then
    print_error "No app name provided"
    echo "Usage: $0 <new-app-name>"
    echo "Example: $0 my-cool-app"
    exit 1
fi

NEW_APP_NAME="$1"
NEW_APP_NAME_PASCAL=$(echo "$NEW_APP_NAME" | sed -r 's/(^|-)([a-z])/\U\2/g')

# Convert PascalCase to space-separated display name
if [[ "$OSTYPE" == "darwin"* ]]; then
    NEW_APP_NAME_DISPLAY=$(echo "$NEW_APP_NAME_PASCAL" | sed -E 's/([a-z])([A-Z])/\1 \2/g')
else
    NEW_APP_NAME_DISPLAY=$(echo "$NEW_APP_NAME_PASCAL" | sed -r 's/([a-z])([A-Z])/\1 \2/g')
fi

# Validate app name format (lowercase with hyphens)
if [[ ! "$NEW_APP_NAME" =~ ^[a-z][a-z0-9-]*$ ]]; then
    print_error "Invalid app name format"
    echo "App name must:"
    echo "  - Start with a lowercase letter"
    echo "  - Contain only lowercase letters, numbers, and hyphens"
    echo "Example: my-cool-app"
    exit 1
fi

print_info "Renaming application from 'blazor-net-app' to '$NEW_APP_NAME'"
print_info "Pascal case name will be: $NEW_APP_NAME_PASCAL"
print_info "Display name will be: $NEW_APP_NAME_DISPLAY"

# Confirm with user
read -p "Continue with rename? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    print_warning "Rename cancelled"
    exit 0
fi

# Current names
OLD_APP_NAME="blazor-net-app"
OLD_APP_NAME_PASCAL="BlazorNetApp"

print_info "Step 1: Renaming directories..."
if [ -d "src/BlazorNetApp.Api" ]; then
    mv "src/BlazorNetApp.Api" "src/${NEW_APP_NAME_PASCAL}.Api"
    print_info "  Renamed src/BlazorNetApp.Api -> src/${NEW_APP_NAME_PASCAL}.Api"
fi

if [ -d "tests/BlazorNetApp.Tests" ]; then
    mv "tests/BlazorNetApp.Tests" "tests/${NEW_APP_NAME_PASCAL}.Tests"
    print_info "  Renamed tests/BlazorNetApp.Tests -> tests/${NEW_APP_NAME_PASCAL}.Tests"
fi

if [ -d "tests/BlazorNetApp.IntegrationTests" ]; then
    mv "tests/BlazorNetApp.IntegrationTests" "tests/${NEW_APP_NAME_PASCAL}.IntegrationTests"
    print_info "  Renamed tests/BlazorNetApp.IntegrationTests -> tests/${NEW_APP_NAME_PASCAL}.IntegrationTests"
fi

print_info "Step 2: Renaming files..."
if [ -f "src/${NEW_APP_NAME_PASCAL}.Api/BlazorNetApp.Api.csproj" ]; then
    mv "src/${NEW_APP_NAME_PASCAL}.Api/BlazorNetApp.Api.csproj" "src/${NEW_APP_NAME_PASCAL}.Api/${NEW_APP_NAME_PASCAL}.Api.csproj"
    print_info "  Renamed BlazorNetApp.Api.csproj -> ${NEW_APP_NAME_PASCAL}.Api.csproj"
fi

if [ -f "tests/${NEW_APP_NAME_PASCAL}.Tests/BlazorNetApp.Tests.csproj" ]; then
    mv "tests/${NEW_APP_NAME_PASCAL}.Tests/BlazorNetApp.Tests.csproj" "tests/${NEW_APP_NAME_PASCAL}.Tests/${NEW_APP_NAME_PASCAL}.Tests.csproj"
    print_info "  Renamed BlazorNetApp.Tests.csproj -> ${NEW_APP_NAME_PASCAL}.Tests.csproj"
fi

if [ -f "tests/${NEW_APP_NAME_PASCAL}.IntegrationTests/BlazorNetApp.IntegrationTests.csproj" ]; then
    mv "tests/${NEW_APP_NAME_PASCAL}.IntegrationTests/BlazorNetApp.IntegrationTests.csproj" "tests/${NEW_APP_NAME_PASCAL}.IntegrationTests/${NEW_APP_NAME_PASCAL}.IntegrationTests.csproj"
    print_info "  Renamed BlazorNetApp.IntegrationTests.csproj -> ${NEW_APP_NAME_PASCAL}.IntegrationTests.csproj"
fi

if [ -f "BlazorNetApp.sln" ]; then
    mv "BlazorNetApp.sln" "${NEW_APP_NAME_PASCAL}.sln"
    print_info "  Renamed BlazorNetApp.sln -> ${NEW_APP_NAME_PASCAL}.sln"
fi

if [ -f "deployment/systemd/blazor-net-app.service" ]; then
    mv "deployment/systemd/blazor-net-app.service" "deployment/systemd/${NEW_APP_NAME}.service"
    print_info "  Renamed blazor-net-app.service -> ${NEW_APP_NAME}.service"
fi

print_info "Step 3: Updating file contents..."

# Function to update file content
update_file() {
    local file="$1"
    if [ -f "$file" ]; then
        # Use sed in a cross-platform way (works on both Linux and macOS)
        if [[ "$OSTYPE" == "darwin"* ]]; then
            # macOS requires an extension argument for -i
            sed -i '' "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" "$file"
            sed -i '' "s/blazor-net-app/${NEW_APP_NAME}/g" "$file"
            sed -i '' "s/Net App/${NEW_APP_NAME_DISPLAY}/g" "$file"
        else
            # Linux doesn't need the extension argument
            sed -i "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" "$file"
            sed -i "s/blazor-net-app/${NEW_APP_NAME}/g" "$file"
            sed -i "s/Net App/${NEW_APP_NAME_DISPLAY}/g" "$file"
        fi
    fi
}

# Update C# files
if [[ "$OSTYPE" == "darwin"* ]]; then
    find src -name "*.cs" -type f -exec sed -i '' "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
    find tests -name "*.cs" -type f -exec sed -i '' "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
else
    find src -name "*.cs" -type f -exec sed -i "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
    find tests -name "*.cs" -type f -exec sed -i "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
fi

# Update project files
if [[ "$OSTYPE" == "darwin"* ]]; then
    find . -name "*.csproj" -type f -exec sed -i '' "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
else
    find . -name "*.csproj" -type f -exec sed -i "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
fi

# Update solution file
if [ -f "${NEW_APP_NAME_PASCAL}.sln" ]; then
    if [[ "$OSTYPE" == "darwin"* ]]; then
        sed -i '' "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" "${NEW_APP_NAME_PASCAL}.sln"
    else
        sed -i "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" "${NEW_APP_NAME_PASCAL}.sln"
    fi
fi

# Update configuration files
update_file "src/${NEW_APP_NAME_PASCAL}.Api/appsettings.json"
update_file "src/${NEW_APP_NAME_PASCAL}.Api/appsettings.Development.json"
update_file "src/${NEW_APP_NAME_PASCAL}.Api/Properties/launchSettings.json"

# Update Razor view files
if [ -d "src/${NEW_APP_NAME_PASCAL}.Api/Pages" ]; then
    if [[ "$OSTYPE" == "darwin"* ]]; then
        find "src/${NEW_APP_NAME_PASCAL}.Api/Pages" -name "*.cshtml" -type f -exec sed -i '' "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
    else
        find "src/${NEW_APP_NAME_PASCAL}.Api/Pages" -name "*.cshtml" -type f -exec sed -i "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
    fi
fi

# Update Blazor component files
if [ -d "src/${NEW_APP_NAME_PASCAL}.Api/Components" ]; then
    if [[ "$OSTYPE" == "darwin"* ]]; then
        find "src/${NEW_APP_NAME_PASCAL}.Api/Components" -name "*.razor" -type f -exec sed -i '' "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
        find "src/${NEW_APP_NAME_PASCAL}.Api/Components" -name "*.razor" -type f -exec sed -i '' "s/blazor-net-app/${NEW_APP_NAME}/g" {} +
        find "src/${NEW_APP_NAME_PASCAL}.Api/Components" -name "*.razor" -type f -exec sed -i '' "s/Net App/${NEW_APP_NAME_DISPLAY}/g" {} +
    else
        find "src/${NEW_APP_NAME_PASCAL}.Api/Components" -name "*.razor" -type f -exec sed -i "s/BlazorNetApp/${NEW_APP_NAME_PASCAL}/g" {} +
        find "src/${NEW_APP_NAME_PASCAL}.Api/Components" -name "*.razor" -type f -exec sed -i "s/blazor-net-app/${NEW_APP_NAME}/g" {} +
        find "src/${NEW_APP_NAME_PASCAL}.Api/Components" -name "*.razor" -type f -exec sed -i "s/Net App/${NEW_APP_NAME_DISPLAY}/g" {} +
    fi
fi

# Update VS Code configuration
update_file ".vscode/settings.json"
update_file ".vscode/launch.json"
update_file ".vscode/tasks.json"

# Update deployment files
update_file "deployment/install.sh"
update_file "deployment/systemd/${NEW_APP_NAME}.service"

# Update GitHub workflows
update_file ".github/workflows/ci-cd.yml"
update_file ".github/workflows/release.yml"

# Update documentation
update_file "README.md"
update_file "CONTRIBUTING.md"
update_file "docs/index.md"

# Update Docker files
update_file "Dockerfile"
update_file "docker-compose.yml"

# Update build scripts
update_file "build-deb.sh"
update_file "build-nuget.ps1"

# Update docker dev scripts
update_file "docker-dev.sh"
update_file "docker-dev.ps1"

# Update debian package files
update_file "debian/DEBIAN/control.template"
update_file "debian/DEBIAN/postinst"
update_file "debian/DEBIAN/prerm"
update_file "debian/DEBIAN/postrm"

# Update IMPLEMENTATION_SUMMARY.md
update_file "IMPLEMENTATION_SUMMARY.md"

print_info "Step 4: Cleaning build artifacts..."
rm -rf src/*/bin src/*/obj tests/*/bin tests/*/obj

print_info "${GREEN}✓${NC} Rename complete!"
echo ""
echo "Next steps:"
echo "  1. Review the changes: git status"
echo "  2. Build the project: dotnet build"
echo "  3. Run tests: dotnet test"
echo "  4. Update README.md with your app-specific information"
echo "  5. Commit changes: git add . && git commit -m 'Rename app to ${NEW_APP_NAME}'"
echo ""
