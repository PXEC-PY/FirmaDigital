import type { SignatureDto } from '../api/types'
import { formatDateTime, truncateMiddle } from '../lib/format'
import { InfoCard, InfoRow } from './InfoCard'
import { StatusBadge } from './StatusBadge'
import { overallStatusLabel, overallStatusTone } from '../lib/statusStyles'

export function SignatureCard({ signature }: { signature: SignatureDto }) {
  return (
    <InfoCard
      title="Firma"
      headerRight={
        <StatusBadge
          label={overallStatusLabel(signature.estado)}
          tone={overallStatusTone(signature.estado)}
        />
      }
    >
      <InfoRow label="Fecha y hora" value={formatDateTime(signature.fechaFirma)} />
      <InfoRow label="Algoritmo de resumen" value={signature.algoritmoResumen} />
      <InfoRow label="Algoritmo de firma" value={signature.algoritmoFirma} />
      <InfoRow
        label="Número de serie"
        value={<span className="font-mono text-xs">{signature.numeroSerie}</span>}
      />
      <InfoRow
        label="Thumbprint"
        value={<span className="font-mono text-xs">{truncateMiddle(signature.thumbprint, 10)}</span>}
      />
      {signature.motivo && (
        <p className="pt-1 text-xs text-slate-500 dark:text-slate-400">{signature.motivo}</p>
      )}
    </InfoCard>
  )
}
