# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY TentacleOracle/TentacleOracle.csproj TentacleOracle/
RUN dotnet restore "TentacleOracle/TentacleOracle.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TentacleOracle"
RUN dotnet build "TentacleOracle.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "TentacleOracle.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app


# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TentacleOracle.dll"]
