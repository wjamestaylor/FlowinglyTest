#!/bin/bash
echo "Starting TextParsingApp..."
echo "PORT: ${PORT:-5000}"
export ASPNETCORE_URLS="http://+:${PORT:-5000}"
echo "ASPNETCORE_URLS: $ASPNETCORE_URLS"
cd /app
echo "Starting .NET application..."
exec dotnet TextParsingApi.dll
