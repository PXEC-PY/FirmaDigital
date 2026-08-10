import type { TimestampDto } from '../api/types'
import { formatDateTime } from '../lib/format'
import { InfoCard, InfoRow } from './InfoCard'
import { StatusBadge } from './StatusBadge'

export function TimestampCard({ timestamp }: { timestamp: TimestampDto }) {
  if (!timestamp.presente) {
    return (
      <InfoCard title="Timestamp">
        <p className="text-sm text-slate-400">
          La firma no incluye un sello de tiempo (RFC 3161).
        </p>
      </InfoCard>
    )
  }

  return (
    <InfoCard
      title="Timestamp"
      headerRight={
        timestamp.valido === null ? null : (
          <StatusBadge
            label={timestamp.valido ? 'Válido' : 'Inválido'}
            tone={timestamp.valido ? 'success' : 'danger'}
          />
        )
      }
    >
      <InfoRow label="Fecha y hora" value={formatDateTime(timestamp.fechaHora)} />
      <InfoRow label="Autoridad de sellado" value={timestamp.autoridadSellado ?? 'No disponible'} />
    </InfoCard>
  )
}
