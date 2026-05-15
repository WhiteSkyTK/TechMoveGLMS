# ── Stage 1: Build ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first — better layer caching on NuGet restore
COPY TechMoveGLMS.API/TechMoveGLMS.API.csproj TechMoveGLMS.API/
COPY TechMoveGLMS/TechMoveGLMS.csproj         TechMoveGLMS/

RUN dotnet restore TechMoveGLMS.API/TechMoveGLMS.API.csproj

# Copy full source and publish
COPY . .
WORKDIR /src/TechMoveGLMS.API
RUN dotnet publish TechMoveGLMS.API.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime (slim image) ─────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
RUN mkdir -p wwwroot/uploads/contracts
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "TechMoveGLMS.API.dll"]
