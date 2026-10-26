# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RegistryServices.csproj ./
RUN dotnet restore RegistryServices.csproj

COPY . .
RUN dotnet publish RegistryServices.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    PORT=8080 \
    BehindProxy=true \
    RunMigrationsOnStartup=true \
    EnableSwagger=false

COPY --from=build /app/publish .

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "RegistryServices.dll"]
