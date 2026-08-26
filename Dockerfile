# syntax=docker/dockerfile:1

# ---- Build stage ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy project files first so package restore is cached across builds
# unless a .csproj actually changes.
COPY src/POSShopTicketing.Domain/POSShopTicketing.Domain.csproj src/POSShopTicketing.Domain/
COPY src/POSShopTicketing.Shared/POSShopTicketing.Shared.csproj src/POSShopTicketing.Shared/
COPY src/POSShopTicketing.Application/POSShopTicketing.Application.csproj src/POSShopTicketing.Application/
COPY src/POSShopTicketing.Infrastructure/POSShopTicketing.Infrastructure.csproj src/POSShopTicketing.Infrastructure/
COPY src/POSShopTicketing.Api/POSShopTicketing.Api.csproj src/POSShopTicketing.Api/
RUN dotnet restore src/POSShopTicketing.Api/POSShopTicketing.Api.csproj

COPY src/ src/
RUN dotnet publish src/POSShopTicketing.Api/POSShopTicketing.Api.csproj \
    -c Release -o /app --no-restore

# ---- Runtime stage ------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Non-root user - Alpine's addgroup/adduser, not useradd/groupadd.
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

COPY --from=build /app .

# Serilog writes one log file per run to ./logs (relative to WORKDIR,
# see Program.cs) - create it now and chown /app as a whole (not just
# the files COPY just placed) so appuser can create that directory - and
# any future files in it - at runtime. WORKDIR alone leaves /app owned
# by root, so skipping this step means the very first log write fails
# with "Permission denied" the moment the container actually starts.
RUN mkdir -p /app/logs && chown -R appuser:appgroup /app

USER appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "POSShopTicketing.Api.dll"]
