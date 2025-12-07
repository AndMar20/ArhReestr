# Базовый образ для рантайма ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Образ для сборки (SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Если нужен прокси — раскомментируй ARG и ENV ниже
# (но сам Dockerfile будет работать и без прокси)

# ARG HTTP_PROXY
# ARG HTTPS_PROXY
# ARG NO_PROXY
# ENV http_proxy=${HTTP_PROXY} \
#     https_proxy=${HTTPS_PROXY} \
#     no_proxy=${NO_PROXY}

# Настройка NuGet на более терпимое поведение при медленном интернете
ENV NUGET_HTTP_TIMEOUT=600 \
    NUGET_DOWNLOAD_TIMEOUT=600 \
    NUGET_ENABLE_ENHANCED_HTTP_RETRY=true \
    NUGET_ENHANCED_HTTP_RETRY_COUNT=6 \
    NUGET_CERT_REVOCATION_MODE=offline

# Копируем только файлы проектов для более быстрого кеширования restore
# (пути от корня контекста сборки)
COPY WebApp/WebApp.csproj WebApp/
COPY DataLayer/DataLayer.csproj DataLayer/

# ВАЖНО: один restore, сразу с "осторожными" настройками
# --disable-parallel уменьшает нагрузку на сеть
# --source явно указывает nuget.org
RUN dotnet restore WebApp/WebApp.csproj \
    --disable-parallel \
    --source https://api.nuget.org/v3/index.json

# Теперь копируем всё остальное
COPY . .
WORKDIR /src/WebApp

# Публикация приложения
RUN dotnet publish WebApp.csproj -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ только с рантаймом и опубликованным приложением
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WebApp.dll"]
