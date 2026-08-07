import type { CertificateDto } from '../api/types'
import { formatDateTime } from '../lib/format'
import { InfoCard, InfoRow } from './InfoCard'
import { StatusBadge } from './StatusBadge'
import { certificateStatusLabel, certificateStatusTone } from '../lib/statusStyles'

export function CertificateCard({ certificate }: { certificate: CertificateDto }) {
  return (
    <InfoCard
      title="Certificado"
      headerRight={
        <StatusBadge
          label={certificateStatusLabel(certificate.estado)}
          tone={certificateStatusTone(certificate.estado)}
        />
      }
    >
      <InfoRow label="Emisor" value={certificate.emisor} />
      <InfoRow label="Autoridad certificadora" value={certificate.autoridadCertificadora} />
      <InfoRow label="Fecha de emisión" value={formatDateTime(certificate.fechaEmision)} />
      <InfoRow label="Fecha de expiración" value={formatDateTime(certificate.fechaExpiracion)} />
    </InfoCard>
  )
}
