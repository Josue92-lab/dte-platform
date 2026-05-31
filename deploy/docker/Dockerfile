# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Restore packages based on CPM
COPY Directory.Packages.props Directory.Build.props ./
COPY src/DTE.Domain/DTE.Domain.csproj src/DTE.Domain/
COPY src/DTE.Application/DTE.Application.csproj src/DTE.Application/
COPY src/DTE.Infrastructure/DTE.Infrastructure.csproj src/DTE.Infrastructure/
COPY src/DTE.Api/DTE.Api.csproj src/DTE.Api/
RUN dotnet restore src/DTE.Api/DTE.Api.csproj
# Copy source and build
COPY src/ src/
RUN dotnet publish src/DTE.Api/DTE.Api.csproj -c Release -o /app/publish /p:UseAppHost=false
# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# Security hardening: Non-root execution
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
# Health Support
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "DTE.Api.dll"]
