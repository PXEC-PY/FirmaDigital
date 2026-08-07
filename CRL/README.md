# CRL/

Listas de revocación de certificados (Certificate Revocation List) locales, usadas como primera
fuente de revocación antes de intentar OCSP o de descargar la CRL desde el CRL Distribution
Point publicado en el certificado. Se cargan desde la carpeta configurada en
`backend/src/ValidadorFirmas.Api/appsettings.json` → `TrustStore:CrlPath`.

## Formato

- Extensión: `.crl` (DER).
- Un archivo por CRL. El emisor se lee del propio archivo (`IssuerDN`); no hace falta nombrar el
  archivo de una forma particular.

## Contenido actual

- `ac_raiz_py.crl` — CRL de la Autoridad Certificadora Raíz del Paraguay.

## Actualización

Las CRL tienen una vigencia limitada (`thisUpdate` / `nextUpdate`). Reemplazar este archivo
periódicamente por una versión más reciente descargada del punto de distribución oficial
(publicado en la propia CRL como Issuing Distribution Point). Si la CRL local falta o está
vencida, el validador intenta descargarla automáticamente desde la URL indicada en la extensión
CRL Distribution Points del certificado que se está validando.

## Agregar la CRL de una nueva Autoridad Certificadora

Igual que con `TrustedCertificates/`: copiar el archivo `.crl` correspondiente a esta carpeta no
requiere cambios de código.
