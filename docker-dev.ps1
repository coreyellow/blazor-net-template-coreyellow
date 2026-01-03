#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Development script for running the application with Docker
.DESCRIPTION
    This script manages Docker containers for local development
.PARAMETER Command
    The command to execute: start, stop, restart, logs, clean, build
.EXAMPLE
    .\docker-dev.ps1
    .\docker-dev.ps1 -Command start
    .\docker-dev.ps1 -Command logs
#>

[CmdletBinding()]
param(
    [Parameter(Position=0)]
    [ValidateSet('start', 'stop', 'restart', 'logs', 'clean', 'build')]
    [string]$Command = 'start'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-ColorOutput {
    param(
        [string]$Message,
        [ConsoleColor]$Color = [ConsoleColor]::White
    )
    Write-Host $Message -ForegroundColor $Color
}

switch ($Command) {
    'start' {
        Write-ColorOutput "Starting blazor-net-app with Docker Compose..." -Color Green
        docker-compose up -d
        Write-Host ""
        Write-ColorOutput "✓ Application started successfully!" -Color Green
        Write-Host ""
        Write-Host "Access the application at:"
        Write-Host "  - Web UI: http://localhost:5000"
        Write-Host "  - Swagger API: http://localhost:5000/swagger"
        Write-Host "  - MQTT Broker: localhost:1883"
        Write-Host ""
        Write-Host "View logs with: .\docker-dev.ps1 -Command logs"
        Write-Host "Stop with: .\docker-dev.ps1 -Command stop"
    }
    
    'stop' {
        Write-ColorOutput "Stopping containers..." -Color Yellow
        docker-compose down
        Write-ColorOutput "✓ Containers stopped" -Color Green
    }
    
    'restart' {
        Write-ColorOutput "Restarting containers..." -Color Yellow
        docker-compose restart
        Write-ColorOutput "✓ Containers restarted" -Color Green
    }
    
    'logs' {
        Write-ColorOutput "Viewing logs (Ctrl+C to exit)..." -Color Cyan
        docker-compose logs -f
    }
    
    'clean' {
        Write-ColorOutput "Stopping containers and removing volumes..." -Color Yellow
        docker-compose down -v
        Write-ColorOutput "✓ Cleanup complete" -Color Green
    }
    
    'build' {
        Write-ColorOutput "Rebuilding Docker image..." -Color Yellow
        docker-compose build --no-cache
        Write-ColorOutput "✓ Image rebuilt successfully" -Color Green
    }
}
