#!/bin/bash
set -e

echo "=== TextParsingApp Startup ==="
echo "Environment:"
echo "  PORT: ${PORT:-5000}"
echo "  ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Production}"
echo "  PWD: $(pwd)"
echo "  User: $(whoami)"

# Set the URL binding
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-5000}"
echo "  ASPNETCORE_URLS: $ASPNETCORE_URLS"

# Check if the DLL exists
if [ ! -f "TextParsingApi.dll" ]; then
    echo "ERROR: TextParsingApi.dll not found in $(pwd)"
    ls -la
    exit 1
fi

echo "Starting .NET application..."
exec dotnet TextParsingApi.dll
