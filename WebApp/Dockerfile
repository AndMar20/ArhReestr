FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Configure NuGet to better handle slow or unstable connections during restore.
# - Increase HTTP/download timeouts to 10 minutes.
# - Enable enhanced retry logic with a higher retry count.
# - Disable certificate revocation checks inside the build container (helps with
#   corporate proxies or restricted environments).
ENV NUGET_HTTP_TIMEOUT=600 \
    NUGET_DOWNLOAD_TIMEOUT=600 \
    NUGET_ENABLE_ENHANCED_HTTP_RETRY=true \
    NUGET_ENHANCED_HTTP_RETRY_COUNT=6 \
    NUGET_CERT_REVOCATION_MODE=offline

# Restore only the projects we need for the web application first
COPY WebApp/WebApp.csproj WebApp/
COPY DataLayer/DataLayer.csproj DataLayer/
# Use conservative restore settings to reduce timeout risk in CI/docker builds.
RUN dotnet restore WebApp/WebApp.csproj --disable-parallel --ignore-failed-sources

# Copy the remaining source
COPY . .
WORKDIR /src/WebApp

# Publish the application
RUN dotnet publish WebApp.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WebApp.dll"]