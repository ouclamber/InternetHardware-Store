# ─── Этап 1: Сборка ──────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала копируем только .csproj — для кэша restore
COPY ["Backend.csproj", "./"]
COPY ["Backend.Domain/Backend.Domain.csproj", "Backend.Domain/"]
COPY ["Backend.Application/Backend.Application.csproj", "Backend.Application/"]
COPY ["Backend.Infrastructure/Backend.Infrastructure.csproj", "Backend.Infrastructure/"]

# Восстанавливаем зависимости
RUN dotnet restore "Backend.csproj"

# Копируем весь код
COPY . .

# Публикуем в Release
RUN dotnet publish "Backend.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ─── Этап 2: Runtime ─────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Устанавливаем curl для healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Создаём non-root пользователя (безопасность)
RUN useradd -m -s /bin/bash appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/api/test || exit 1

ENTRYPOINT ["dotnet", "Backend.dll"]