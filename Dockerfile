# Multi-stage Dockerfile for AAQSOLS Heart Clinic HMS API (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY ["src/Domain/HeartClinicHms.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/HeartClinicHms.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Api/HeartClinicHms.Api.csproj", "src/Api/"]

# Restore dependencies
RUN dotnet restore "src/Api/HeartClinicHms.Api.csproj"

# Copy remaining source code and publish
COPY src/ ./src/
WORKDIR /src/src/Api
RUN dotnet publish "HeartClinicHms.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Default to Demo environment with embedded SQLite database
ENV ASPNETCORE_ENVIRONMENT=Demo
ENV UseSqlite=true
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HeartClinicHms.Api.dll"]
