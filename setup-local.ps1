<#
.SYNOPSIS
    Sets up Ed-Fi AdminAPI multi-tenant Docker environment
.DESCRIPTION
    This script runs the Docker Compose files to set up a multi-tenant Ed-Fi environment
    with ODS databases and AdminAPI containers.
.PARAMETER EnvFile
    Path to the environment file (default: .env)
.PARAMETER Down
    Switch to bring down the containers instead of starting them
.PARAMETER Build
    Switch to force rebuild of containers
.PARAMETER Logs
    Switch to show logs after starting containers
#>

param(
    [string]$EnvFile = ".env",
    [switch]$Down,
    [switch]$Build,
    [switch]$Logs
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Define paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ComposeDir = Join-Path $ScriptDir "Docker\Compose\pgsql\MultiTenant"
$OdsComposeFile = Join-Path $ComposeDir "compose-build-ods-multi-tenant.yml"
$DevComposeFile = Join-Path $ComposeDir "compose-build-dev-multi-tenant.yml"

# Function to check if Docker is running
function Test-DockerRunning {
    try {
        docker info | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

# Function to check if file exists
function Test-FileExists {
    param([string]$FilePath, [string]$Description)

    if (-not (Test-Path $FilePath)) {
        Write-Error "$Description not found at: $FilePath"
        exit 1
    }
    Write-Host "✓ Found $Description" -ForegroundColor Green
}

# Function to run Docker Compose command with multiple files
function Invoke-MultiDockerCompose {
    param(
        [string[]]$ComposeFiles,
        [string]$Command,
        [string]$Description
    )

    Write-Host "$Description..." -ForegroundColor Cyan

    # Build the docker-compose command with multiple -f flags
    $cmd = "docker-compose"

    # Add multiple -f flags for each compose file
    foreach ($file in $ComposeFiles) {
        $cmd += " -f `"$file`""
        Write-Host "Using compose file: $file" -ForegroundColor Gray
    }

    # Add environment file if it exists
    if (Test-Path $EnvFile) {
        $cmd += " --env-file `"$EnvFile`""
        Write-Host "Using environment file: $EnvFile" -ForegroundColor Gray
    }

    $cmd += " $Command"

    Write-Host "Executing: $cmd" -ForegroundColor Gray

    try {
        Invoke-Expression $cmd
        if ($LASTEXITCODE -eq 0) {
            Write-Host "$Description completed successfully" -ForegroundColor Green
        } else {
            Write-Error "$Description failed with exit code: $LASTEXITCODE"
        }
    }
    catch {
        Write-Error "Failed to execute $Description`: $_"
        exit 1
    }
}

# Main execution
try {
    Write-Host "=== Ed-Fi AdminAPI Multi-Tenant Docker Setup ===" -ForegroundColor Yellow
    Write-Host "Timestamp: $(Get-Date)" -ForegroundColor Gray

    # Verify prerequisites
    Write-Host "Checking prerequisites..." -ForegroundColor Cyan

    if (-not (Test-DockerRunning)) {
        Write-Error "Docker is not running. Please start Docker Desktop and try again."
        exit 1
    }
    Write-Host "✓ Docker is running" -ForegroundColor Green

    # Check if docker-compose is available
    try {
        docker-compose --version | Out-Null
        Write-Host "✓ Docker Compose is available" -ForegroundColor Green
    }
    catch {
        Write-Error "Docker Compose is not available. Please install Docker Compose."
        exit 1
    }

    # Verify compose files exist
    Test-FileExists -FilePath $DevComposeFile -Description "Dev compose file"
    Test-FileExists -FilePath $OdsComposeFile -Description "ODS compose file"

    # Check environment file
    if (Test-Path $EnvFile) {
        Write-Host "Using environment file: $EnvFile" -ForegroundColor Green
    } else {
        Write-Warning "Environment file not found: $EnvFile"
        Write-Host "Continuing with default Docker environment variables..." -ForegroundColor Yellow
    }

    # Define compose files array (order matters - Dev first, then ODS)
    $ComposeFiles = @($DevComposeFile, $OdsComposeFile)

    if ($Down) {
        # Bring down containers
        Write-Host "Bringing down containers..." -ForegroundColor Red

        Invoke-MultiDockerCompose -ComposeFiles $ComposeFiles -Command "down --remove-orphans --volumes" -Description "Stopping all containers and removing volumes"

        Write-Host "All containers have been stopped and removed" -ForegroundColor Green
    }
    else {
        # Start containers
        Write-Host "Starting multi-tenant environment..." -ForegroundColor Green

        # Build command
        $upCommand = "up -d"
        if ($Build) {
            $upCommand += " --build"
            Write-Host "Building containers..." -ForegroundColor Yellow
        }

        # Start all containers together
        Invoke-MultiDockerCompose -ComposeFiles $ComposeFiles -Command $upCommand -Description "Starting multi-tenant containers"

        # Wait for all services to be ready
        Write-Host "Waiting for all services to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 20

        # Show container status
        Write-Host "Container Status:" -ForegroundColor Cyan
        docker ps --filter "name=ed-fi" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

        # Check health of containers
        Write-Host "`Health Check:" -ForegroundColor Cyan
        $unhealthyContainers = docker ps --filter "name=ed-fi" --filter "health=unhealthy" --format "{{.Names}}"
        if ($unhealthyContainers) {
            Write-Warning "Some containers are unhealthy: $unhealthyContainers"
        } else {
            Write-Host "✓ All containers appear healthy" -ForegroundColor Green
        }

        # Show logs if requested
        if ($Logs) {
            Write-Host "Container Logs:" -ForegroundColor Cyan
            Write-Host "Press Ctrl+C to stop following logs..." -ForegroundColor Gray

            # Use the same compose files for logs
            $logCmd = "docker-compose"
            foreach ($file in $ComposeFiles) {
                $logCmd += " -f `"$file`""
            }
            if (Test-Path $EnvFile) {
                $logCmd += " --env-file `"$EnvFile`""
            }
            $logCmd += " logs -f"

            Invoke-Expression $logCmd
        }

        Write-Host "Multi-tenant environment setup completed!" -ForegroundColor Green
        Write-Host "Access points:" -ForegroundColor Cyan
        Write-Host "  • AdminAPI: http://localhost:5001" -ForegroundColor White
        Write-Host "  • Swagger: http://localhost:5001/swagger" -ForegroundColor White
        Write-Host "  • Health: http://localhost:5001/health" -ForegroundColor White
        Write-Host "Useful commands:" -ForegroundColor Cyan
        Write-Host "  • View logs: .\setup-local.ps1 -Logs" -ForegroundColor White
        Write-Host "  • Stop all: .\setup-local.ps1 -Down" -ForegroundColor White
        Write-Host "  • Rebuild: .\setup-local.ps1 -Build" -ForegroundColor White
        Write-Host "  • Status: docker ps --filter name=ed-fi" -ForegroundColor White
    }
}
catch {
    Write-Error "Script execution failed: $_"
    Write-Host "Troubleshooting tips:" -ForegroundColor Yellow
    Write-Host "  1. Ensure Docker Desktop is running" -ForegroundColor White
    Write-Host "  2. Check if ports 5001, 5432 are available" -ForegroundColor White
    Write-Host "  3. Verify .env file contains required variables" -ForegroundColor White
    Write-Host "  4. Try running with -Build flag to rebuild containers" -ForegroundColor White
    Write-Host "  5. Check container logs: docker-compose -f Dev -f ODS logs" -ForegroundColor White
    exit 1
}
