# Blazor .NET Template

[![.NET Version](https://img.shields.io/badge/.NET-10.0%2B-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0%2B-512BD4.svg)](https://learn.microsoft.com/aspnet/core)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0%2B-512BD4.svg)](https://learn.microsoft.com/ef/core)
[![MQTTnet](https://img.shields.io/badge/MQTTnet-5.0%2B-00979D.svg)](https://github.com/dotnet/MQTTnet)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3%2B-7952B3.svg)](https://getbootstrap.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A comprehensive .NET template featuring REST API, MQTT support, SQLite database, OpenAPI/Swagger documentation, Blazor Server UI built with ASP.NET Core, and Template scrips.

![Blazor .NET Template](blazor-net-template.png)

## 📖 Table of Contents

- [📚 Documentation](#-documentation)
- [Features](#features)
- [Customizing the Application Name](#customizing-the-application-name)
- [Quick Start](#quick-start)
  - [Prerequisites](#prerequisites)
  - [Running Locally](#running-locally)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
  - [appsettings.json](#appsettingsjson)
  - [Environment Variables](#environment-variables)
- [Docker Development](#docker-development)
  - [Available Commands](#available-commands)
  - [Development Workflow](#development-workflow)
  - [Testing MQTT with Docker](#testing-mqtt-with-docker)
- [MQTT Integration](#mqtt-integration)
  - [Configuration](#configuration-1)
  - [Commands](#commands)
  - [Publishing Events](#publishing-events)
  - [Testing](#testing)
- [Production Deployment](#production-deployment)
  - [Using Docker (Recommended)](#using-docker-recommended-for-production)
  - [Using NuGet Package (Windows)](#using-nuget-package-windows)
  - [Using DEB Package (Ubuntu/Debian)](#using-deb-package-ubuntudebian)
  - [Manual Installation](#manual-installation)
- [Development](#development)
- [GitHub Actions](#github-actions)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)
- [Support](#support)

## 📚 Documentation

Full documentation is available on [GitHub Pages](https://mlmdevs.github.io/blazor-net-template/):

- [README](https://mlmdevs.github.io/blazor-net-template/README.html) - Getting started and features
- [Contributing Guide](https://mlmdevs.github.io/blazor-net-template/CONTRIBUTING.html) - How to contribute
- [Implementation Summary](https://mlmdevs.github.io/blazor-net-template/IMPLEMENTATION_SUMMARY.html) - Technical details

## Features

### 🚀 Core Features

- **REST API** - Full CRUD operations with ASP.NET Core Web API
- **MQTT Support** - Pub/sub messaging with MQTTnet
- **SQLite Database** - Entity Framework Core with migrations
- **OpenAPI/Swagger** - Interactive API documentation
- **Web Interface** - Blazor Server UI
- **Unit Tests** - xUnit with FluentAssertions and Moq
- **Integration Tests** - WebApplicationFactory for API testing

### 🛠️ Development

- **Code Formatting** - EditorConfig for consistent code style
- **VS Code Ready** - Launch and task configurations included
- **JetBrains Rider Ready** - Run configurations for debugging, testing, and building
- **GitHub Actions** - CI/CD with automated testing and builds
- **Hot Reload** - dotnet watch for rapid development
- **Setup Scripts** - Automated dependency installation scripts
- **Git Hooks** - Pre-commit formatting for consistent code quality

### 📦 Deployment

- **Docker Images** - Multi-architecture images published to GitHub Packages (Container Registry)
- **Docker Compose** - Local development with MQTT broker included
- **DEB Packages** - Automated builds for amd64 and arm64, pushed to APT repository
- **NuGet Packages** - .NET Global Tool for Windows installation
- **Systemd Service** - Production deployment on Ubuntu/Debian

### 🔧 Customization

- **Rename Script** - Easy app renaming with bash or PowerShell scripts
- **Configurable** - Customize all aspects of the template

## Customizing the Application Name

The template comes with scripts to easily rename the application from "blazor-net-app" to your own application name.

### Using Bash (Linux/macOS)

```bash
./rename-app.sh my-cool-app
```

### Using PowerShell (Windows)

```powershell
.\Rename-App.ps1 -NewAppName my-cool-app
```

The scripts will:

- Rename all directories and files
- Update namespaces in C# files
- Update configuration files (including MQTT topic prefixes)
- Update deployment scripts
- Update documentation
- Update UI display names and titles
- Update Blazor component files

**Note:** The app name should be in kebab-case (lowercase with hyphens, e.g., `my-cool-app`).

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite (included)
- Optional: MQTT broker (e.g., Mosquitto) for MQTT features
- Optional: Docker and Docker Compose for containerized development

**Quick Setup**: Use the provided setup script to install all dependencies automatically:

```bash
# Install all dependencies at once (Linux/macOS)
./scripts/setup.sh
```

Or install dependencies individually:

```bash
./scripts/install-dotnet10.sh    # .NET 10 SDK
./scripts/install-sqlite3.sh     # SQLite3
./scripts/install-mosquitto.sh   # MQTT broker
./scripts/install-docker.sh      # Docker and Docker Compose
```

### Running Locally

#### Option 1: Using Docker (Recommended for Development)

1. **Clone the repository**

   ```bash
   git clone https://github.com/mlmdevs/blazor-net-template.git
   cd blazor-net-template
   ```

2. **Start with Docker Compose**

   **Linux/macOS:**

   ```bash
   ./docker-dev.sh start
   ```

   **Windows (PowerShell):**

   ```powershell
   .\docker-dev.ps1 -Command start
   ```

   This will:

   - Build the Docker image
   - Start the application container
   - Start a Mosquitto MQTT broker
   - Create persistent volumes for data

3. **Access the application**

   - Web UI: http://localhost:5000
   - Swagger API: http://localhost:5000/swagger
   - MQTT Broker: localhost:1883

4. **View logs**

   ```bash
   ./docker-dev.sh logs        # Linux/macOS
   .\docker-dev.ps1 logs       # Windows
   ```

5. **Stop containers**
   ```bash
   ./docker-dev.sh stop        # Linux/macOS
   .\docker-dev.ps1 stop       # Windows
   ```

See the [Docker Development](#docker-development) section for more commands.

#### Option 2: Using .NET CLI

1. **Clone the repository**

   ```bash
   git clone https://github.com/mlmdevs/blazor-net-template.git
   cd blazor-net-template
   ```

2. **Set up git hooks (optional but recommended)**

   ```bash
   ./.githooks/install.sh
   ```

   This installs a pre-commit hook that automatically formats code before commits.

3. **Restore dependencies**

   ```bash
   dotnet restore
   ```

4. **Run the application**

   ```bash
   dotnet run --project src/BlazorNetApp.Api
   ```

5. **Access the application**
   - Web UI: https://localhost:5001 or http://localhost:5000
   - Swagger API: https://localhost:5001/swagger

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/BlazorNetApp.Tests
dotnet test tests/BlazorNetApp.IntegrationTests
```

### VS Code

1. Open the project in VS Code
2. Install recommended extensions when prompted
3. Press `F5` to start debugging
4. The application will open automatically in your browser

### JetBrains Rider

1. Open the project in JetBrains Rider (open the `BlazorNetApp.sln` file)
2. The project includes pre-configured run configurations for:
   - **BlazorNetApp.Api (Debug)** - Run/debug the main API project
   - **All Unit Tests** - Run unit tests
   - **All Integration Tests** - Run integration tests
   - **All Tests (Unit + Integration)** - Run all tests
   - **Build Solution** - Build the entire solution
   - **Build NuGet Package** - Create a NuGet package
   - **Build Docker Image** - Build a Docker image
   - **Build Debian Package (amd64)** - Create a Debian package
3. Select a configuration from the dropdown in the toolbar and press `F5` to run/debug
4. Use `Ctrl+Shift+F10` (Windows/Linux) or `Cmd+Shift+F10` (macOS) to run the current test

## Project Structure

```
blazor-net-template/
├── src/
│   └── BlazorNetApp.Api/                    # Main API project
│       ├── Controllers/               # API controllers
│       ├── Data/                      # Database context
│       ├── Models/                    # Data models
│       ├── Pages/                     # Razor Pages for UI
│       ├── Services/                  # Background services (MQTT)
│       └── wwwroot/                   # Static files (CSS, JS)
├── tests/
│   ├── BlazorNetApp.Tests/                 # Unit tests
│   └── BlazorNetApp.IntegrationTests/      # Integration tests
├── scripts/                           # Setup and install scripts
│   ├── setup.sh                       # Master setup script
│   ├── install-dotnet10.sh            # .NET 10 installation
│   ├── install-sqlite3.sh             # SQLite3 installation
│   ├── install-mosquitto.sh           # MQTT broker installation
│   └── install-docker.sh              # Docker installation
├── deployment/
│   ├── systemd/                       # Systemd service files
│   └── install.sh                     # Installation script
├── .githooks/                         # Git hooks for code formatting
│   ├── pre-commit                     # Pre-commit formatting hook
│   └── install.sh                     # Hook installation script
├── .github/workflows/                 # GitHub Actions workflows
│   ├── ci-cd.yml                      # Build and test workflow
│   └── release.yml                    # Release and publish workflow
├── .vscode/                           # VS Code configuration
├── Dockerfile                         # Docker image build
├── docker-compose.yml                 # Docker Compose setup
├── mosquitto.conf                     # MQTT broker config
├── docker-dev.sh                      # Docker dev script (Linux/macOS)
├── docker-dev.ps1                     # Docker dev script (Windows)
├── rename-app.sh                      # Rename script (Bash)
├── Rename-App.ps1                     # Rename script (PowerShell)
├── build-deb.sh                       # DEB package build script
├── build-nuget.ps1                    # NuGet package build script
└── .editorconfig                      # Code style settings
```

## API Endpoints

### TodoItems

- `GET /api/todoitems` - Get all TODO items
- `GET /api/todoitems/{id}` - Get a specific TODO item
- `POST /api/todoitems` - Create a new TODO item
- `PUT /api/todoitems/{id}` - Update a TODO item
- `DELETE /api/todoitems/{id}` - Delete a TODO item

### Swagger Documentation

- `GET /swagger` - Interactive API documentation

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=blazor-net-app.db"
  },
  "Mqtt": {
    "Enabled": false,
    "Broker": "localhost",
    "Port": 1883,
    "ClientId": "blazor-net-app",
    "Topic": "blazor-net-app/#"
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Set to `Development` or `Production`
- `ASPNETCORE_URLS` - Override default URLs (e.g., `http://0.0.0.0:5000`)

## Docker Development

The project includes convenient scripts for Docker-based development with an integrated MQTT broker.

### Available Commands

**Linux/macOS:**

```bash
./docker-dev.sh [command]
```

**Windows (PowerShell):**

```powershell
.\docker-dev.ps1 -Command [command]
```

### Commands

- `start` - Build and start containers (default)
- `stop` - Stop all containers
- `restart` - Restart containers
- `logs` - View container logs (Ctrl+C to exit)
- `clean` - Stop containers and remove volumes
- `build` - Rebuild Docker image from scratch

### What's Included

When you run `docker-dev.sh start` or `docker-dev.ps1 start`, it will:

1. Build the .NET application Docker image
2. Start the blazor-net-app container on port 5000
3. Start a Mosquitto MQTT broker on port 1883
4. Create persistent Docker volumes for:
   - Application database (`blazor-net-app-data`)
   - MQTT broker data (`mosquitto-data`)
   - MQTT broker logs (`mosquitto-logs`)

### Accessing Services

- **Web UI:** http://localhost:5000
- **Swagger API:** http://localhost:5000/swagger
- **MQTT Broker:** localhost:1883
- **MQTT WebSockets:** ws://localhost:9001

### Development Workflow

```bash
# Start development environment
./docker-dev.sh start

# Make code changes in your editor

# Rebuild and restart to test changes
./docker-dev.sh build
./docker-dev.sh restart

# View logs to debug
./docker-dev.sh logs

# Clean up when done
./docker-dev.sh clean
```

### Testing MQTT with Docker

When using Docker, the MQTT broker is automatically configured and accessible. Test it using mosquitto clients:

```bash
# Subscribe to responses
mosquitto_sub -h localhost -t "blazor-net-app/response/#" -v

# Send a command (in another terminal)
mosquitto_pub -h localhost -t "blazor-net-app/command/getall" \
  -m '{"correlationId":"test-123"}'
```

## MQTT Integration

The template includes comprehensive MQTT support for pub/sub messaging. When enabled, the application:

- Publishes events when TODO items are created, updated, or deleted
- Receives and processes commands via MQTT topics
- Supports all CRUD operations through MQTT (same as REST API)

### Enabling MQTT

1. Install and run an MQTT broker (e.g., Mosquitto)
2. Update `appsettings.json` to set `Mqtt:Enabled` to `true`
3. Configure broker address and port as needed

### MQTT Command Topics

The application subscribes to `blazor-net-app/command/#` and supports the following commands:

#### Get All TODO Items

**Topic:** `blazor-net-app/command/getall`
**Payload:**

```json
{
  "correlationId": "optional-request-id"
}
```

#### Get TODO Item by ID

**Topic:** `blazor-net-app/command/get`
**Payload:**

```json
{
  "id": 1,
  "correlationId": "optional-request-id"
}
```

#### Create TODO Item

**Topic:** `blazor-net-app/command/create`
**Payload:**

```json
{
  "title": "New TODO Item",
  "description": "Optional description",
  "isCompleted": false,
  "correlationId": "optional-request-id"
}
```

#### Update TODO Item

**Topic:** `blazor-net-app/command/update`
**Payload:**

```json
{
  "id": 1,
  "title": "Updated Title",
  "description": "Updated description",
  "isCompleted": true,
  "correlationId": "optional-request-id"
}
```

#### Delete TODO Item

**Topic:** `blazor-net-app/command/delete`
**Payload:**

```json
{
  "id": 1,
  "correlationId": "optional-request-id"
}
```

### MQTT Responses

Responses are published to:

- `blazor-net-app/response/{correlationId}` if a correlationId was provided
- `blazor-net-app/response` otherwise

**Success Response:**

```json
{
  "success": true,
  "data": {
    /* returned object */
  }
}
```

**Error Response:**

```json
{
  "success": false,
  "error": "Error message"
}
```

### Testing with Mosquitto

Install mosquitto clients:

```bash
# Ubuntu/Debian
sudo apt-get install mosquitto-clients

# macOS
brew install mosquitto
```

Subscribe to responses:

```bash
mosquitto_sub -h localhost -t "blazor-net-app/response/#" -v
```

Send a command (in another terminal):

```bash
# Create a TODO item
mosquitto_pub -h localhost -t "blazor-net-app/command/create" \
  -m '{"title":"Test via MQTT","description":"Testing","correlationId":"test-123"}'

# Get all TODO items
mosquitto_pub -h localhost -t "blazor-net-app/command/getall" \
  -m '{"correlationId":"test-456"}'

# Update a TODO item
mosquitto_pub -h localhost -t "blazor-net-app/command/update" \
  -m '{"id":1,"title":"Updated","isCompleted":true,"correlationId":"test-789"}'

# Delete a TODO item
mosquitto_pub -h localhost -t "blazor-net-app/command/delete" \
  -m '{"id":1,"correlationId":"test-999"}'
```

## Production Deployment

### Using Docker (Recommended for Production)

Docker images are automatically built and published to GitHub Container Registry on each release, supporting both `amd64` and `arm64` architectures.

#### Pull and Run from GitHub Packages

1. **Log in to GitHub Container Registry**

   ```bash
   echo $GITHUB_PAT | docker login ghcr.io -u USERNAME --password-stdin
   ```

   Replace `GITHUB_PAT` with a [GitHub Personal Access Token](https://github.com/settings/tokens) with `read:packages` permission, and `USERNAME` with your GitHub username.

2. **Pull the image**

   ```bash
   docker pull ghcr.io/mlmdevs/blazor-net-template:latest
   ```

3. **Run the container**

   ```bash
   docker run -d \
     --name blazor-net-app \
     -p 8080:8080 \
     -v blazor-net-app-data:/app/data \
     -e ASPNETCORE_ENVIRONMENT=Production \
     -e Mqtt__Enabled=false \
     ghcr.io/mlmdevs/blazor-net-template:latest
   ```

4. **Access the application**
   - Web UI: http://localhost:8080
   - Swagger API: http://localhost:8080/swagger

#### Docker Compose for Production

Create a `docker-compose.prod.yml`:

```yaml
version: "3.8"

services:
  blazor-net-app:
    image: ghcr.io/mlmdevs/blazor-net-template:latest
    container_name: blazor-net-app
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Mqtt__Enabled=true
      - Mqtt__Broker=mosquitto
    volumes:
      - blazor-net-app-data:/app/data
    restart: unless-stopped

  mosquitto:
    image: eclipse-mosquitto:2
    container_name: blazor-net-app-mosquitto
    ports:
      - "1883:1883"
    volumes:
      - mosquitto-data:/mosquitto/data
      - ./mosquitto.conf:/mosquitto/config/mosquitto.conf:ro
    restart: unless-stopped

volumes:
  blazor-net-app-data:
  mosquitto-data:
```

Then run:

```bash
docker-compose -f docker-compose.prod.yml up -d
```

#### Environment Variables

Configure the application using environment variables:

- `ASPNETCORE_ENVIRONMENT` - Set to `Production`
- `ASPNETCORE_URLS` - Override URLs (default: `http://+:8080`)
- `ConnectionStrings__DefaultConnection` - SQLite database path
- `Mqtt__Enabled` - Enable/disable MQTT (`true`/`false`)
- `Mqtt__Broker` - MQTT broker hostname
- `Mqtt__Port` - MQTT broker port (default: `1883`)
- `Mqtt__ClientId` - MQTT client ID
- `Mqtt__Topic` - MQTT topic pattern

### Using NuGet Package (Windows)

#### Install as .NET Global Tool

1. **Configure GitHub Packages as a NuGet source** (one-time setup)

   ```powershell
   dotnet nuget add source "https://nuget.pkg.github.com/mlmdevs/index.json" `
     --name "GitHub-mlmdevs" `
     --username YOUR_GITHUB_USERNAME `
     --password YOUR_GITHUB_PAT `
     --store-password-in-clear-text
   ```

   Replace `YOUR_GITHUB_USERNAME` with your GitHub username and `YOUR_GITHUB_PAT` with a [GitHub Personal Access Token](https://github.com/settings/tokens) with `read:packages` permission.

2. **Install the tool**

   ```powershell
   dotnet tool install --global BlazorNetApp --version <version>
   ```

3. **Run the application**

   ```powershell
   blazor-net-app
   ```

   The application will start and be accessible at:

   - Web UI: https://localhost:5001 or http://localhost:5000
   - Swagger API: https://localhost:5001/swagger

   **Note:** The database file (`blazor-net-app.db`) will be created in your current working directory.

4. **Update to a newer version**

   ```powershell
   dotnet tool update --global BlazorNetApp
   ```

5. **Uninstall**
   ```powershell
   dotnet tool uninstall --global BlazorNetApp
   ```

#### Install from Release Assets

Alternatively, you can download the `.nupkg` file from the [releases page](https://github.com/mlmdevs/blazor-net-template/releases) and install it locally:

```powershell
dotnet tool install --global --add-source ./path/to/folder BlazorNetApp
```

Replace `./path/to/folder` with the directory containing the downloaded `.nupkg` file.

### Using DEB Package (Ubuntu/Debian)

1. **Download the latest release** from the [releases page](https://github.com/mlmdevs/blazor-net-template/releases)

2. **Install the package**

   ```bash
   sudo dpkg -i blazor-net-app_<version>_<arch>.deb
   ```

   Note: The DEB package is self-contained and includes .NET 10 runtime. No additional dependencies are required.

   The service runs in Production mode and stores the database at `/var/lib/blazor-net-app/blazor-net-app.db`.

3. **Manage the service**

   ```bash
   sudo systemctl status blazor-net-app
   sudo systemctl restart blazor-net-app
   sudo systemctl stop blazor-net-app
   sudo systemctl start blazor-net-app
   ```

4. **View logs**
   ```bash
   sudo journalctl -u blazor-net-app -f
   ```

### Manual Installation

1. **Publish the application**

   ```bash
   dotnet publish src/BlazorNetApp.Api/BlazorNetApp.Api.csproj \
     --configuration Release \
     --output publish \
     --runtime linux-x64 \
     --self-contained true
   ```

2. **Run the installation script**
   ```bash
   sudo ./deployment/install.sh
   ```

## Development

### Adding New Features

1. **Create a new model** in `src/BlazorNetApp.Api/Models/`
2. **Add DbSet** to `ApplicationDbContext`
3. **Create a controller** in `src/BlazorNetApp.Api/Controllers/`
4. **Update UI** in `src/BlazorNetApp.Api/Pages/` and `wwwroot/`
5. **Write tests** in `tests/`

### Code Style

The project uses EditorConfig for consistent code formatting.

**Format check:**

```bash
dotnet format --verify-no-changes
```

**Auto-format code:**

```bash
dotnet format
```

**Pre-commit hook (automatic formatting):**

Set up the pre-commit hook to automatically format code before each commit:

```bash
./.githooks/install.sh
```

After installation, any commit will trigger automatic code formatting. If issues are found, they will be fixed automatically, and you'll need to review and stage the changes before committing again.

### Database Migrations

```bash
# Create a migration
cd src/BlazorNetApp.Api
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update
```

## GitHub Actions

The project includes two workflows:

1. **CI/CD** (`ci-cd.yml`) - Runs on push/PR

   - Builds the solution
   - Runs all tests
   - Checks code formatting
   - Publishes artifacts

2. **Release** (`release.yml`) - Runs on version tags
   - Builds and pushes Docker images (amd64 & arm64) to GitHub Container Registry
   - Builds DEB packages for amd64 and arm64
   - Pushes DEB packages to APT repository
   - Builds and pushes NuGet package to GitHub Packages
   - Creates GitHub release with all packages

To create a release:

```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built with [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- MQTT support via [MQTTnet](https://github.com/dotnet/MQTTnet)
- UI powered by [Bootstrap 5](https://getbootstrap.com)
- Testing with [xUnit](https://xunit.net) and [FluentAssertions](https://fluentassertions.com)

## Support

For issues and questions:

- [GitHub Issues](https://github.com/mlmdevs/blazor-net-template/issues)
- [Documentation](https://github.com/mlmdevs/blazor-net-template/wiki)
