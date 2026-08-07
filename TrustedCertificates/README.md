# TrustedCertificates/

Certificados de la(s) PKI en las que confía el validador. Se cargan en memoria al iniciar la
API (`FileSystemTrustedCertificateStore`) desde la carpeta configurada en
`backend/src/ValidadorFirmas.Api/appsettings.json` → `TrustStore:RootCertificatesPath`.

## Formato

- Extensiones admitidas: `.cer`, `.crt`, `.pem`, `.der` (DER o PEM, se detecta automáticamente).
- Un archivo por certificado.
- El validador clasifica automáticamente cada archivo como **raíz** o **intermedio** mirando si
  el certificado está autofirmado (Subject == Issuer). No hace falta separarlos en subcarpetas
  ni indicarlo en configuración.

## Contenido actual

- `ac_raiz_py_sha256.crt` — Autoridad Certificadora Raíz del Paraguay (autofirmado, ancla de confianza).
- Certificados intermedios de los Prestadores Cualificados de Servicios de Confianza acreditados
  ante el Ministerio de Industria y Comercio (DOCUMENTA S.A., CODE100 S.A., CONFIRMA S.A.,
  ITTI SAECA, SOS TECNOLOGIA, VIT S.A., Ministerio del Interior), todos emitidos por la raíz
  anterior.

## Agregar una nueva Autoridad Certificadora (Paraguay u otro país)

No requiere cambios de código: alcanza con copiar el certificado raíz (y, si corresponde, los
intermedios) a esta carpeta y reiniciar la API. Esto es lo que permite que el sistema escale a
otras PKI nacionales (Brasil, Argentina, Uruguay, Chile, Perú, etc.) simplemente agregando sus
certificados aquí y la/s CRL correspondiente/s en `CRL/`.

## Si esta carpeta está vacía

El sistema no falla: registra una advertencia al iniciar y cada validación reporta la cadena de
confianza como "No verificable" en lugar de "Correcta" o "Incorrecta", hasta que se agreguen
certificados raíz reales.
