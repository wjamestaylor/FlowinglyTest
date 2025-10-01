# Multi-stage Dockerfile for TextParsingApp on Railway
# Stage 1: Build React Frontend
FROM node:18-alpine AS frontend-build

WORKDIR /app/frontend
COPY src/text-parsing-ui/package*.json ./
RUN npm ci --only=production

COPY src/text-parsing-ui/ ./
RUN npm run build

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build

WORKDIR /app/backend
COPY src/TextParsingApi/*.csproj ./
RUN dotnet restore

COPY src/TextParsingApi/ ./
RUN dotnet publish -c Release -o out

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy the built API
COPY --from=backend-build /app/backend/out ./

# Copy the built React app to wwwroot for serving
COPY --from=frontend-build /app/frontend/build ./wwwroot

# Copy startup script
COPY start.sh ./
RUN chmod +x start.sh

# Set environment variables for Railway
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
ENV DOTNET_URLS=http://+:5000

# Expose the port (Railway dynamically assigns PORT)
EXPOSE 5000

# Start the application using startup script
CMD ["./start.sh"]
