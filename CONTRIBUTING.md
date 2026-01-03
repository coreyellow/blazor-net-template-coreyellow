# Contributing to Blazor .NET Template

Thank you for your interest in contributing to the Blazor .NET Template! This document provides guidelines and instructions for contributing.

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please be respectful and constructive in all interactions.

## How to Contribute

### Reporting Bugs

If you find a bug, please create an issue on GitHub with:
- A clear, descriptive title
- Steps to reproduce the issue
- Expected behavior
- Actual behavior
- Environment details (.NET version, OS, etc.)
- Any relevant logs or error messages

### Suggesting Enhancements

Enhancement suggestions are welcome! Please create an issue with:
- A clear, descriptive title
- Detailed description of the proposed feature
- Use cases and benefits
- Any implementation ideas you might have

### Pull Requests

1. **Fork and Clone**
   ```bash
   git clone https://github.com/YOUR-USERNAME/blazor-net-template.git
   cd blazor-net-template
   ```

2. **Create a Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Set up Git Hooks (Recommended)**
   ```bash
   ./.githooks/install.sh
   ```
   This installs a pre-commit hook that automatically formats code before commits.

4. **Make Your Changes**
   - Follow the existing code style
   - Add tests for new functionality
   - Update documentation as needed
   - Ensure all tests pass

4. **Test Your Changes**
   ```bash
   dotnet test
   dotnet format --verify-no-changes
   ```

5. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "Add feature: description of your feature"
   ```
   
   Note: If you installed the pre-commit hook, code will be automatically formatted before commit.

6. **Push and Create PR**
   ```bash
   git push origin feature/your-feature-name
   ```
   Then create a Pull Request on GitHub.

## Development Guidelines

### Setting Up Development Environment

The project includes helpful setup scripts for installing dependencies:

```bash
# Install all dependencies at once
./scripts/setup.sh

# Or install individually:
./scripts/install-dotnet10.sh    # .NET 10 SDK
./scripts/install-sqlite3.sh     # SQLite3
./scripts/install-mosquitto.sh   # MQTT broker
./scripts/install-docker.sh      # Docker and Docker Compose
```

### Git Hooks

The project includes a pre-commit hook that automatically formats code:

```bash
# Install the git hooks
./.githooks/install.sh
```

After installation:
- Code will be automatically formatted with `dotnet format` before each commit
- If formatting issues are found, they'll be fixed automatically
- You'll need to review and stage the changes before committing again

### Docker Development

For development with Docker:

```bash
# Linux/macOS
./docker-dev.sh start    # Start containers
./docker-dev.sh logs     # View logs
./docker-dev.sh stop     # Stop containers

# Windows (PowerShell)
.\docker-dev.ps1 start
.\docker-dev.ps1 logs
.\docker-dev.ps1 stop
```

### Code Style

- Follow .NET naming conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- The project uses EditorConfig for formatting

### Testing

- Write unit tests for business logic
- Write integration tests for API endpoints
- Aim for good code coverage
- Tests should be fast and reliable
- Use meaningful test names that describe the scenario

### Commit Messages

- Use clear, descriptive commit messages
- Start with a verb in present tense (Add, Fix, Update, Remove)
- Reference issues when applicable (#123)
- Keep the first line under 50 characters
- Add detailed description if needed

Example:
```
Add MQTT reconnection logic

- Implement exponential backoff for reconnection
- Add configuration for retry attempts
- Update tests to cover reconnection scenarios

Fixes #123
```

### Documentation

- Update README.md for user-facing changes
- Add XML comments for public APIs
- Update code comments for complex logic
- Include examples where helpful

## Project Structure

```
blazor-net-template/
├── src/
│   └── BlazorNetApp.Api/          # Main application
├── tests/
│   ├── BlazorNetApp.Tests/        # Unit tests
│   └── BlazorNetApp.IntegrationTests/  # Integration tests
├── scripts/                        # Setup and install scripts
├── deployment/                     # Deployment files
├── .githooks/                      # Git hooks
├── .github/workflows/              # CI/CD workflows
├── .vscode/                        # VS Code configuration
├── Dockerfile                      # Docker image build
├── docker-compose.yml              # Docker Compose setup
├── docker-dev.sh/.ps1              # Docker dev scripts
└── rename-app.sh/Rename-App.ps1    # Rename scripts
```

## Getting Help

- Check existing issues and pull requests
- Review the README.md and documentation
- Create an issue for questions or discussions

## Recognition

Contributors will be acknowledged in release notes and the project README.

Thank you for contributing! 🎉
