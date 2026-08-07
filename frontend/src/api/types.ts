// Tipos espejados a mano desde los DTOs de
// backend/src/ValidadorFirmas.Application/Dtos. Mantener sincronizados manualmente.

export type OverallStatus = 'Valido' | 'Invalido' | 'Advertencia'
export type CertificateStatus = 'Vigente' | 'Revocado' | 'Expirado' | 'Desconocido'
export type ChainStatus = 'Correcta' | 'Incorrecta' | 'NoVerificable'
export type RevocationStatus = 'NoRevocado' | 'Revocado' | 'NoVerificable'
export type RevocationSource = 'Ninguna' | 'Ocsp' | 'Crl'

export interface SignerDto {
  nombreCompleto: string
  numeroDocumento: string | null
  correo: string | null
  empresa: string | null
  cargo: string | null
}

export interface ChainDto {
  estado: ChainStatus
  motivo: string | null
  cadenaEmisores: string[]
}

export interface RevocationDto {
  estado: RevocationStatus
  fuente: RevocationSource
  fechaConsulta: string | null
  motivo: string | null
}

export interface CertificateDto {
  emisor: string
  autoridadCertificadora: string
  fechaEmision: string
  fechaExpiracion: string
  numeroSerie: string
  thumbprint: string
  estado: CertificateStatus
  cadena: ChainDto
  revocacion: RevocationDto
}

export interface TimestampDto {
  presente: boolean
  fechaHora: string | null
  autoridadSellado: string | null
  valido: boolean | null
}

export interface SignatureDto {
  nombreCampo: string
  firmante: SignerDto
  fechaFirma: string | null
  algoritmoResumen: string
  algoritmoFirma: string
  numeroSerie: string
  thumbprint: string
  certificado: CertificateDto
  timestamp: TimestampDto
  integridadCriptograficaValida: boolean
  cubreDocumentoCompleto: boolean
  estado: OverallStatus
  motivo: string | null
}

export interface DocumentIntegrityDto {
  esIntegro: boolean
  cantidadFirmas: number
  motivo: string | null
}

export interface DocumentValidationResponseDto {
  documentoId: string
  nombreArchivo: string
  hashSha256: string
  fechaValidacion: string
  estadoGeneral: OverallStatus
  motivo: string
  documento: DocumentIntegrityDto
  firmas: SignatureDto[]
}

export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: string[]
}
