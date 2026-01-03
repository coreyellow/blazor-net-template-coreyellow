# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["BlazorNetApp.sln", "./"]
COPY ["src/BlazorNetApp.Api/BlazorNetApp.Api.csproj", "src/BlazorNetApp.Api/"]
COPY ["tests/BlazorNetApp.Tests/BlazorNetApp.Tests.csproj", "tests/BlazorNetApp.Tests/"]
COPY ["tests/BlazorNetApp.IntegrationTests/BlazorNetApp.IntegrationTests.csproj", "tests/BlazorNetApp.IntegrationTests/"]

# Restore dependencies
RUN dotnet restore "BlazorNetApp.sln"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /src/src/BlazorNetApp.Api
RUN dotnet build "BlazorNetApp.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BlazorNetApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create a non-root user
RUN groupadd -r netapp && useradd -r -g netapp netapp

# Create directory for SQLite database with proper permissions
RUN mkdir -p /app/data && chown -R netapp:netapp /app/data

# Copy published files
COPY --from=publish /app/publish .

# Change ownership of app files
RUN chown -R netapp:netapp /app

# Switch to non-root user
USER netapp

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/blazor-net-app.db"

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/api/todoitems || exit 1

ENTRYPOINT ["dotnet", "BlazorNetApp.Api.dll"]
