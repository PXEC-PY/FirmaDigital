# Validador de Firmas Digitales del Paraguay — v1

Núcleo funcional de punta a punta: subir un PDF firmado, validarlo de verdad (integridad
criptográfica, cadena de confianza contra la PKI paraguaya, revocación por OCSP/CRL, sello de
tiempo) y ver el resultado en el navegador.

Arquitectura completa (Clean Architecture / DDD / CQRS), decisiones de diseño y alcance
detallado en [`docs/PLAN.md`](docs/PLAN.md).

## Estructura

```
backend/    ASP.NET Core 10 · Clean Architecture (Domain/Application/Infrastructure/Api/Shared)
frontend/   React 19 · TypeScript · Vite · TailwindCSS · React Query · React Hook Form · Zod
TrustedCertificates/   Certificados raíz e intermedios de la PKI paraguaya (ya cargados)
CRL/                   Listas de revocación locales (ya cargada la de la raíz)
```

## Requisitos

- .NET SDK 10 (`dotnet --version`)
- Node.js 20+ y npm
- Una base PostgreSQL (local o gratis en [Supabase](https://supabase.com)/[Neon](https://neon.tech))

## Variables de entorno (backend)

El backend falla al arrancar si faltan. Se cargan como variables de entorno (formato
`Seccion__Clave`), nunca en `appsettings.json`:

| Variable | Descripción |
|---|---|
| `Jwt__SigningKey` | Clave de firma JWT, mínimo 32 caracteres. Generarla con `openssl rand -base64 48`. |
| `ConnectionStrings__Postgres` | Cadena de conexión a Postgres, ej. `Host=...;Database=...;Username=...;Password=...`. |
| `Admin__Email` / `Admin__InitialPassword` | Credenciales del administrador inicial (solo se usan si la tabla `Users` está vacía). |
| `Cors__AllowedOrigins__0` | Origen permitido para el frontend (en local: `http://localhost:5173`). |

## Cómo correrlo

**Backend** (puerto 5214):

```bash
cd backend
export Jwt__SigningKey="..." ConnectionStrings__Postgres="..." Admin__Email="..." Admin__InitialPassword="..."
dotnet run --project src/ValidadorFirmas.Api
```

Swagger UI en `http://localhost:5214/swagger` (solo en entorno Development).

**Frontend** (puerto 5173):

```bash
cd frontend
npm install
npm run dev
```

El frontend apunta a `http://localhost:5214/api/v1` por defecto (`frontend/.env`).

## Deploy

`render.yaml` en la raíz define el Blueprint para desplegar en [Render](https://render.com)
(API como Web Service Docker + frontend como Static Site), con Postgres externo
(Supabase/Neon). Ver instrucciones paso a paso en el mensaje de configuración del
proyecto o en `render.yaml`.

## Tests

```bash
cd backend
dotnet test
```

## Qué valida (v1)

Integridad criptográfica de cada firma, si el documento fue modificado después de firmarse,
cadena de confianza contra los certificados en `TrustedCertificates/`, estado de revocación
(OCSP si el certificado publica un responder, si no CRL local o remota), vigencia del
certificado, algoritmo usado, sello de tiempo RFC 3161 si existe, y firmas múltiples.

## Qué queda para una fase 2

Historial en base de datos (SQL Server + EF Core), autenticación JWT con roles, generación de
informe PDF con QR, dashboard con gráficos, auditoría y rate limiting. La arquitectura ya deja
los puntos de extensión listos para agregarlo sin tocar el dominio de validación.
