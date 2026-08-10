# Build context: raíz del repo (necesita TrustedCertificates/ y CRL/, que viven
# fuera de backend/). En Render: dockerContext "." y dockerfilePath "./backend/Dockerfile".

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/ValidadorFirmas.Api/ValidadorFirmas.Api.csproj backend/src/ValidadorFirmas.Api/
COPY backend/src/ValidadorFirmas.Application/ValidadorFirmas.Application.csproj backend/src/ValidadorFirmas.Application/
COPY backend/src/ValidadorFirmas.Domain/ValidadorFirmas.Domain.csproj backend/src/ValidadorFirmas.Domain/
COPY backend/src/ValidadorFirmas.Infrastructure/ValidadorFirmas.Infrastructure.csproj backend/src/ValidadorFirmas.Infrastructure/
COPY backend/src/ValidadorFirmas.Shared/ValidadorFirmas.Shared.csproj backend/src/ValidadorFirmas.Shared/
RUN dotnet restore backend/src/ValidadorFirmas.Api/ValidadorFirmas.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/ValidadorFirmas.Api/ValidadorFirmas.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Npgsql requiere libgssapi para autenticación, ausente en la imagen aspnet slim.
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .
COPY TrustedCertificates /app/TrustedCertificates
COPY CRL /app/CRL

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ValidadorFirmas.Api.dll"]
