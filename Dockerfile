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
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "PostgraduateManagement.dll"]
