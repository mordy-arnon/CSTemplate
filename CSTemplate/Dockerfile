# Use official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["CSTemplate.csproj", "./"]
RUN dotnet restore "CSTemplate.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "CSTemplate.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CSTemplate.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for healthchecks (optional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 5000
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/weatherforecast || exit 1

# Run the application
ENTRYPOINT ["dotnet", "CSTemplate.dll"]

