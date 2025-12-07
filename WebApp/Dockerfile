FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore only the projects we need for the web application first
COPY WebApp/WebApp.csproj WebApp/
COPY DataLayer/DataLayer.csproj DataLayer/
RUN dotnet restore WebApp/WebApp.csproj

# Copy the remaining source
COPY . .
WORKDIR /src/WebApp

# Publish the application
RUN dotnet publish WebApp.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WebApp.dll"]
