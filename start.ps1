# P25Scanner Startup Script
param (
    [switch]$Debug,
    [string]$Config = "appsettings.json"
)

# Ensure we're in the right directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Create logs directory if it doesn't exist
if (-not (Test-Path "logs")) {
    New-Item -ItemType Directory -Path "logs"
}

# Set environment variables
$env:DOTNET_ENVIRONMENT = if ($Debug) { "Development" } else { "Production" }

# Build and run the application
Write-Host "Building P25Scanner..."
dotnet build --configuration Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "Starting P25Scanner..."
    if ($Debug) {
        dotnet run --configuration Debug -- --config $Config
    } else {
        dotnet run --configuration Release -- --config $Config
    }
} else {
    Write-Host "Build failed. Please check the error messages above." -ForegroundColor Red
}

