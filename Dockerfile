# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy csproj first
COPY TentacleOracle/TentacleOracle.csproj TentacleOracle/

# Restore dependencies
RUN dotnet restore "TentacleOracle/TentacleOracle.csproj"

# Copy everything else
COPY . .

# Publish app
WORKDIR /src/TentacleOracle

RUN dotnet publish "TentacleOracle.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Create database directory
RUN mkdir -p /app/data

# Copy published app
COPY --from=build /app/publish .

# ASP.NET Core settings
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "TentacleOracle.dll"]