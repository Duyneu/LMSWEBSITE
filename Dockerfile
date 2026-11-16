# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY PostgraduateManagement/PostgraduateManagement.csproj PostgraduateManagement/
RUN dotnet restore PostgraduateManagement/PostgraduateManagement.csproj

# Copy remaining source code and publish
COPY PostgraduateManagement/ PostgraduateManagement/
WORKDIR /src/PostgraduateManagement
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway routes traffic via PORT environment variable
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet PostgraduateManagement.dll --urls http://0.0.0.0:${PORT:-8080}"]
