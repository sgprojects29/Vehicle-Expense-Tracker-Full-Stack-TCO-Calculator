# ============================================
# Stage 1: Test Stage
# Run all tests - build fails if tests fail
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS test
WORKDIR /app

# Copy solution file
COPY VehicleExpenseAPI.sln ./

# Copy project files
COPY src/VehicleExpenseAPI/VehicleExpenseAPI.csproj ./src/VehicleExpenseAPI/
COPY tests/VehicleExpenseAPI.Tests/VehicleExpenseAPI.Tests.csproj ./tests/VehicleExpenseAPI.Tests/

# Restore dependencies for both main and test projects
RUN dotnet restore VehicleExpenseAPI.sln

# Copy all source code
COPY src/ ./src/
COPY tests/ ./tests/

# Run tests - this will fail the Docker build if any test fails
RUN dotnet test tests/VehicleExpenseAPI.Tests/VehicleExpenseAPI.Tests.csproj \
    --configuration Release \
    --verbosity normal \
    --logger "console;verbosity=detailed"

# ============================================
# Stage 2: Build Stage
# Compile the application
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY VehicleExpenseAPI.sln ./
COPY src/VehicleExpenseAPI/VehicleExpenseAPI.csproj ./src/VehicleExpenseAPI/

# Restore dependencies
RUN dotnet restore src/VehicleExpenseAPI/VehicleExpenseAPI.csproj

# Copy source code
COPY src/VehicleExpenseAPI/ ./src/VehicleExpenseAPI/

# Build the application
RUN dotnet build src/VehicleExpenseAPI/VehicleExpenseAPI.csproj \
    --configuration Release \
    --no-restore

# Publish the application
RUN dotnet publish src/VehicleExpenseAPI/VehicleExpenseAPI.csproj \
    --configuration Release \
    --no-build \
    --output /app/publish

# ============================================
# Stage 3: Runtime Stage
# Minimal runtime image for production
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application from build stage
COPY --from=build /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "VehicleExpenseAPI.dll"]
