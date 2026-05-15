# ── Stage 1: Build ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TechMoveGLMS/TechMoveGLMS.csproj         TechMoveGLMS/
COPY TechMoveGLMS.API/TechMoveGLMS.API.csproj TechMoveGLMS.API/

RUN dotnet restore TechMoveGLMS/TechMoveGLMS.csproj

COPY . .
WORKDIR /src/TechMoveGLMS
RUN dotnet publish TechMoveGLMS.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "TechMoveGLMS.dll"]
