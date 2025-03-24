#!/bin/bash

# P25Scanner Startup Script
DEBUG=0
CONFIG="appsettings.json"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --debug)
            DEBUG=1
            shift
            ;;
        --config)
            CONFIG="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Ensure we're in the right directory
cd "$(dirname "$0")"

# Create logs directory if it doesn't exist
mkdir -p logs

# Set environment variables
if [ $DEBUG -eq 1 ]; then
    export DOTNET_ENVIRONMENT="Development"
else
    export DOTNET_ENVIRONMENT="Production"
fi

# Build and run the application
echo "Building P25Scanner..."
dotnet build --configuration Release

if [ $? -eq 0 ]; then
    echo "Starting P25Scanner..."
    if [ $DEBUG -eq 1 ]; then
        dotnet run --configuration Debug -- --config "$CONFIG"
    else
        dotnet run --configuration Release -- --config "$CONFIG"
    fi
else
    echo "Build failed. Please check the error messages above."
    exit 1
fi

