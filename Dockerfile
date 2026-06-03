# ── Stage 1: Build ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies first (layer cache)
COPY ["CleanrApp.csproj", "."]
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install SQLite runtime libs
RUN apt-get update && apt-get install -y --no-install-recommends \
    libsqlite3-0 \
    && rm -rf /var/lib/apt/lists/*

# Create persistent data directory for SQLite file
RUN mkdir -p /app/Data && chmod 755 /app/Data

# Copy published files
COPY --from=build /app/publish .

# Uploads directory for photos
RUN mkdir -p /app/wwwroot/uploads/photos && chmod 755 /app/wwwroot/uploads

# Render.com uses PORT env var (default 10000)
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 10000

ENTRYPOINT ["dotnet", "CleanrApp.dll"]
