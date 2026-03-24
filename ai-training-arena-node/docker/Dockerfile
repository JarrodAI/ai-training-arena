# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/AITrainingArena.Domain/AITrainingArena.Domain.csproj src/AITrainingArena.Domain/
COPY src/AITrainingArena.Application/AITrainingArena.Application.csproj src/AITrainingArena.Application/
COPY src/AITrainingArena.Infrastructure/AITrainingArena.Infrastructure.csproj src/AITrainingArena.Infrastructure/
COPY src/AITrainingArena.BattleEngine/AITrainingArena.BattleEngine.csproj src/AITrainingArena.BattleEngine/
COPY src/AITrainingArena.Blockchain/AITrainingArena.Blockchain.csproj src/AITrainingArena.Blockchain/
COPY src/AITrainingArena.API/AITrainingArena.API.csproj src/AITrainingArena.API/
RUN dotnet restore src/AITrainingArena.API/AITrainingArena.API.csproj

COPY src/ src/
RUN dotnet publish src/AITrainingArena.API/AITrainingArena.API.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

EXPOSE 4001 8080 8081

COPY --from=build /app/publish .

VOLUME ["/app/data", "/app/models"]

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD curl -f http://localhost:8081/health/live || exit 1

ENTRYPOINT ["dotnet", "AITrainingArena.API.dll"]
