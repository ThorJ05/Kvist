# Build context must be the repository root:
#   docker build -f API/Dockerfile -t kvist .

# ---------- 1. Frontend (Bun + React) ----------
FROM oven/bun:1 AS frontend
WORKDIR /web
COPY package.json bun.lock bunfig.toml tsconfig.json ./
RUN bun install --frozen-lockfile
COPY src ./src
RUN bun run build
# output: /web/dist

# ---------- 2. Backend build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# copy only the project files first so the restore layer is cached
COPY API/API.csproj API/
COPY Service/Service.csproj Service/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
RUN dotnet restore API/API.csproj
COPY API/ API/
COPY Service/ Service/
COPY Infrastructure/ Infrastructure/
RUN dotnet publish API/API.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------- 3. Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# SQLite folder that the non-root user may write to (mount a volume here)
RUN mkdir /data && chown $APP_UID:$APP_UID /data
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__DefaultConnection="Data Source=/data/kvist.db"

COPY --from=build /app/publish .
# Program.cs serves the React build from wwwroot
COPY --from=frontend /web/dist ./wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]