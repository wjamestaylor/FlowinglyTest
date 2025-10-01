#!/bin/bash
export ASPNETCORE_URLS="http://+:${PORT:-5000}"
cd /app
exec dotnet TextParsingApi.dll
