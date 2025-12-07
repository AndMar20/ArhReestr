FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# ARG HTTP_PROXY
# ARG HTTPS_PROXY
# ARG NO_PROXY
# ENV http_proxy=${HTTP_PROXY} \
#     https_proxy=${HTTPS_PROXY} \
#     no_proxy=${NO_PROXY}

ENV NUGET_HTTP_TIMEOUT=600 \
    NUGET_DOWNLOAD_TIMEOUT=600 \
    NUGET_ENABLE_ENHANCED_HTTP_RETRY=true \
    NUGET_ENHANCED_HTTP_RETRY_COUNT=6 \
    NUGET_CERT_REVOCATION_MODE=offline

COPY WebApp/WebApp.csproj WebApp/
COPY DataLayer/DataLayer.csproj DataLayer/

RUN dotnet restore WebApp/WebApp.csproj \
    --disable-parallel \
    --source https://api.nuget.org/v3/index.json

COPY . .
WORKDIR /src/WebApp

RUN dotnet publish WebApp.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WebApp.dll"]
