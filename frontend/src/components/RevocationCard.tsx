import type { RevocationDto } from '../api/types'
import { formatDateTime } from '../lib/format'
import { InfoCard, InfoRow } from './InfoCard'
import { StatusBadge } from './StatusBadge'
import { revocationStatusLabel, revocationStatusTone } from '../lib/statusStyles'

const SOURCE_LABEL: Record<RevocationDto['fuente'], string> = {
  Ninguna: 'No consultada',
  Ocsp: 'OCSP',
  Crl: 'CRL',
}

export function RevocationCard({ revocation }: { revocation: RevocationDto }) {
  return (
    <InfoCard
      title="Revocación"
      headerRight={
        <StatusBadge
          label={revocationStatusLabel(revocation.estado)}
          tone={revocationStatusTone(revocation.estado)}
        />
      }
    >
      <InfoRow label="Fuente" value={SOURCE_LABEL[revocation.fuente]} />
      <InfoRow label="Consultado" value={formatDateTime(revocation.fechaConsulta)} />
      {revocation.motivo && (
        <p className="pt-1 text-xs text-slate-500 dark:text-slate-400">{revocation.motivo}</p>
      )}
    </InfoCard>
  )
}
