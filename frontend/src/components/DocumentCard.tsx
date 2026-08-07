import type { DocumentIntegrityDto } from '../api/types'
import { truncateMiddle } from '../lib/format'
import { InfoCard, InfoRow } from './InfoCard'
import { StatusBadge } from './StatusBadge'

interface DocumentCardProps {
  fileName: string
  hashSha256: string
  integrity: DocumentIntegrityDto
}

export function DocumentCard({ fileName, hashSha256, integrity }: DocumentCardProps) {
  return (
    <InfoCard title="Documento">
      <InfoRow label="Nombre del archivo" value={fileName} />
      <InfoRow
        label="Integridad"
        value={
          <StatusBadge
            label={integrity.esIntegro ? 'Documento íntegro' : 'Documento alterado'}
            tone={integrity.esIntegro ? 'success' : 'danger'}
          />
        }
      />
      <InfoRow label="Cantidad de firmas" value={integrity.cantidadFirmas} />
      <InfoRow
        label="Hash SHA-256"
        value={<span className="font-mono text-xs">{truncateMiddle(hashSha256, 12)}</span>}
      />
    </InfoCard>
  )
}
