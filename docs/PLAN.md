# Validador de Firmas Digitales del Paraguay — v1 (Núcleo funcional end-to-end)

## Contexto

Proyecto nuevo (carpeta vacía). El pedido original es una plataforma empresarial completa
(auth, dashboard, historial en SQL Server, informes PDF con QR, OCSP, auditoría, rate
limiting, multi-país). Dado el tamaño, se acordó con el usuario construir primero un
**núcleo funcional real de punta a punta**: subir un PDF firmado, validarlo de verdad
(criptografía + cadena de confianza + CRL/OCSP + timestamp) y mostrar el resultado en el
frontend. Sin auth, sin dashboard, sin base de datos/historial todavía — eso queda para una
fase 2 explícita, sobre esta misma arquitectura (Clean Architecture ya deja los puntos de
extensión listos: agregar EF Core a Infrastructure, JWT a la Api, y nuevas queries de
historial a Application, sin tocar Domain).

Entorno confirmado: .NET SDK 10 instalado (no hay SDK/runtime 9) → los proyectos apuntan a
`net10.0`. Node 24 / npm 11 instalados. Los certificados raíz/CRL reales de la CA paraguaya
los va a copiar el usuario después en `TrustedCertificates/` y `CRL/`; hasta entonces el
sistema debe funcionar y reportar honestamente "cadena de confianza: no verificable" en vez
de fallar.

## Estructura de carpetas

```
/backend
  ValidadorFirmas.sln
  /src
    ValidadorFirmas.Domain          (entidades, enums, value objects, sin dependencias)
    ValidadorFirmas.Application     (CQRS con MediatR, DTOs, interfaces de puertos, FluentValidation)
    ValidadorFirmas.Infrastructure  (iText8 + BouncyCastle.Cryptography: extracción/validación real)
    ValidadorFirmas.Api             (ASP.NET Core 10 Web API, Program.cs, Swagger, middleware)
    ValidadorFirmas.Shared          (Result<T>, excepciones base, constantes)
  /tests
    ValidadorFirmas.Application.Tests
    ValidadorFirmas.Infrastructure.Tests
/frontend
  (Vite + React 18 + TypeScript + Tailwind + React Query + React Hook Form + Zod)
/TrustedCertificates
  README.md   (formato esperado: .cer/.crt/.pem, cómo agregarlos, no se versiona el contenido real)
/CRL
  README.md   (formato esperado: .crl, cómo agregarlos)
```

Referencias de carpetas de certificados/CRL configurables vía `appsettings.json`
(`TrustStore:RootCertificatesPath`, `TrustStore:CrlPath`), no hardcodeadas — así agregar una
nueva CA (Brasil, Argentina, etc.) en el futuro es copiar un archivo, no tocar código.

## Backend — Domain

Entidades/VOs centrados en el resultado de validación (preparados para ser persistidos en
fase 2, pero sin EF Core todavía):

- `SignatureValidation` (entidad): algoritmo, fecha/hora firma, número de serie, thumbprint, estado.
- `SignerInfo` (VO): nombre, documento, email, empresa, cargo — extraídos del DN del certificado
  (best-effort: CN, SERIALNUMBER, E/SAN rfc822Name, O, OU). Sin el perfil oficial de OIDs de la
  CA paraguaya, el mapeo es genérico y queda documentado como punto a refinar cuando lleguen
  certificados reales.
- `CertificateInfo` (VO): emisor, autoridad certificadora, fechas emisión/expiración, estado
  (Vigente/Revocado/Expirado), resultado de cadena (Correcta/Incorrecta/NoVerificable).
- `DocumentIntegrity` (VO): íntegro/alterado, cantidad de firmas.
- Enums: `OverallStatus` (Valido/Invalido/Advertencia), `RevocationStatus`
  (NoRevocado/Revocado/NoVerificable), `ChainStatus`, `CertificateStatus`.
- `DocumentValidationResult` (raíz de agregado): documento + lista de `SignatureValidation` +
  estado general + motivo (texto exacto tipo "El certificado fue revocado.").

## Backend — Application (CQRS vía MediatR)

- `ValidatePdfSignatureCommand` (bytes del PDF + nombre archivo) → `DocumentValidationResponseDto`,
  manejado por `ValidatePdfSignatureCommandHandler`, que orquesta los puertos de Infrastructure
  en orden: extraer firmas → por cada firma: verificar integridad criptográfica → construir cadena
  → verificar CRL/OCSP → verificar timestamp → mapear a DTO → agregar estado general.
- Puertos (interfaces que implementa Infrastructure):
  `IPdfSignatureExtractor`, `ICertificateChainValidator`, `IRevocationChecker`,
  `ITrustedCertificateStore`, `ITimestampValidator`.
- FluentValidation: `ValidatePdfSignatureCommandValidator` (tamaño ≤ 20MB, content-type/magic
  bytes `%PDF-`).
- DTOs con la forma exacta pedida en "DATOS A MOSTRAR": `DocumentValidationResponseDto`,
  `SignatureDto`, `SignerDto`, `CertificateDto`, `ChainDto`, `DocumentIntegrityDto`.

## Backend — Infrastructure (la parte crítica: validación real)

Paquetes NuGet: `itext7`, `itext7.bouncy-castle-adapter`, `BouncyCastle.Cryptography`
(paquete moderno mantenido, no el `Portable.BouncyCastle` obsoleto).

- `PdfSignatureExtractor` (implementa `IPdfSignatureExtractor`): abre el PDF con
  `iText.Kernel.Pdf.PdfReader` + `SignatureUtil`, itera `GetSignatureNames()`, por cada firma
  usa `PdfPKCS7`:
  - `VerifySignatureIntegrityAndAuthenticity()` → integridad criptográfica de esa firma.
  - `SignatureUtil.SignatureCoversWholeDocument(name)` → detecta si hubo cambios incrementales
    después de firmar (documento alterado).
  - `GetSignDate()`, `GetDigestAlgorithm()`/`GetSignatureMechanismName()` → algoritmo real
    (SHA256/SHA384 + RSA/ECDSA), `GetSigningCertificate()` + `GetCertificates()` → cadena cruda.
- `X509ChainValidator` (implementa `ICertificateChainValidator`): convierte certs BouncyCastle →
  `X509Certificate2`, arma `X509Chain` con `X509ChainTrustMode.CustomRootTrust` usando los
  certificados cargados por `ITrustedCertificateStore`, `RevocationMode = NoCheck` (la
  revocación la maneja `IRevocationChecker` aparte para poder combinar CRL local + OCSP),
  interpreta `ChainStatus` → Correcta/Incorrecta + motivo textual.
- `FileSystemTrustedCertificateStore`: carga `.cer/.crt/.pem` desde
  `TrustStore:RootCertificatesPath`; si la carpeta está vacía, loguea advertencia y la cadena
  se reporta como "No verificable" (no crashea).
- `RevocationChecker` (implementa `IRevocationChecker`): por cada cert de la cadena, intenta
  1) OCSP si el cert tiene AIA con OCSP responder (BouncyCastle `Ocsp.*`, request/response
  RFC 6960 real por HTTP), 2) si no, CRL local en `CRL:CrlPath` o la URL del CRL Distribution
  Point del certificado (BouncyCastle `X509Crl`), 3) si ninguna disponible → estado
  "No verificable" (Advertencia, no Inválido).
- `TimestampValidator`: si `PdfPKCS7.GetTimeStampToken()` no es null, valida el token RFC3161
  contra el hash firmado y reporta fecha/hora de sellado.
- Todo async donde hay I/O real (HTTP para OCSP/CRL remoto), con `ILogger<T>` inyectado y
  manejo de excepciones que degrada a "No verificable" en vez de tirar 500.

## Backend — Api

- `Program.cs`: DI de todos los puertos, Swagger/OpenAPI, CORS habilitado para el origen del
  frontend, límite de request body 20MB, Serilog (consola + archivo), middleware global de
  excepciones que devuelve un `ProblemDetails` consistente.
- `POST /api/v1/validations` (multipart/form-data, campo `file`): valida tipo/tamaño,
  despacha `ValidatePdfSignatureCommand` vía MediatR, devuelve `DocumentValidationResponseDto`.
- Sin autenticación en v1 (decisión explícita del usuario) — no se agregan stubs de JWT/roles
  para no generar código muerto; queda anotado como fase 2.

## Backend — Shared

`Result<T>` / `Result` para flujo de errores sin excepciones de control, excepciones base
(`DomainException`, `ValidationException`), constantes (límite de tamaño, extensiones válidas).

## Tests

- `Application.Tests`: validador FluentValidation (tamaño/tipo), mapeo de agregación de estado
  general (una firma inválida → documento inválido; solo revocación no verificable → advertencia).
- `Infrastructure.Tests`: parsing de un PDF firmado de prueba (autofirmado, generado en el test)
  para validar que `PdfSignatureExtractor` detecta la firma e integridad correctamente, y que
  detectar una modificación posterior al firmado marca el documento como alterado.

## Frontend

- Vite + React 18 + TypeScript + TailwindCSS, React Query (mutación de subida), React Hook
  Form + Zod (validación cliente: PDF, ≤20MB antes de enviar).
- Diseño: mucho espacio en blanco, bordes redondeados, sombras suaves, tipografía tipo
  Microsoft/GitHub/Apple, animaciones discretas (Tailwind transitions, sin librerías pesadas).
- Página principal: header con logo + título + descripción, zona drag&drop central, botones
  "Seleccionar PDF" / "Validar".
- Durante la validación: componente de pasos animados con los textos pedidos ("Leyendo
  documento...", "Extrayendo firmas...", etc.) — como el backend responde en una sola llamada,
  los pasos se muestran como progreso simulado ligado a la duración real de la request (no
  fake-fijo), documentado como mejora futura si se quiere streaming real (SSE) del backend.
- Resultado: tarjeta superior grande verde/roja con el estado y el motivo exacto, y tarjetas
  por sección (Firmante, Documento, Firma, Certificado, Cadena, Revocación, Hash, Integridad)
  reflejando 1:1 los DTOs del backend. Tipos TS espejados a mano desde los DTOs de C#.
- Sin dashboard, sin historial, sin login en v1.

## Fuera de alcance de v1 (fase 2, explícitamente pospuesto)

SQL Server + EF Core + historial, JWT + roles + auditoría + rate limiting, generación de
informe PDF con QR, dashboard con gráficos, soporte multi-país (estructura ya lo permite
agregando certificados, pero no se construyen las otras CAs ahora).

## Verificación

1. `dotnet build` sobre la solución completa sin errores/warnings de análisis.
2. `dotnet test` sobre ambos proyectos de test, en verde.
3. Generar (en el test de Infrastructure) o conseguir un PDF firmado de prueba y correr el
   flujo completo vía `dotnet run` en la Api + `curl`/Swagger UI contra
   `POST /api/v1/validations`, confirmando que el JSON de respuesta tiene la forma esperada.
4. `npm run dev` en `/frontend`, subir ese mismo PDF de prueba desde el navegador y confirmar
   que la tarjeta de resultado se renderiza con los datos reales devueltos por la Api (no
   mocks), incluyendo el caso esperado "cadena de confianza no verificable" mientras
   `TrustedCertificates/` esté vacío.
