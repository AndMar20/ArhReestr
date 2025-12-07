FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["WebApp/WebApp.csproj", "WebApp/"]
COPY ["DataLayer/DataLayer.csproj", "DataLayer/"]

COPY offline-packages/ /offline-packages/

ENV NUGET_HTTP_TIMEOUT=600 \
    NUGET_DOWNLOAD_TIMEOUT=600 \
    NUGET_ENABLE_ENHANCED_HTTP_RETRY=true \
    NUGET_ENHANCED_HTTP_RETRY_COUNT=6 \
    NUGET_CERT_REVOCATION_MODE=offline

COPY . .

WORKDIR "/src/WebApp"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]
