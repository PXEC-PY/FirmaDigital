import type { ChainDto } from '../api/types'
import { InfoCard } from './InfoCard'
import { StatusBadge } from './StatusBadge'
import { chainStatusLabel, chainStatusTone } from '../lib/statusStyles'

export function ChainCard({ chain }: { chain: ChainDto }) {
  return (
    <InfoCard
      title="Cadena de confianza"
      headerRight={<StatusBadge label={chainStatusLabel(chain.estado)} tone={chainStatusTone(chain.estado)} />}
    >
      {chain.cadenaEmisores.length > 0 ? (
        <ol className="space-y-1.5 text-sm text-slate-700 dark:text-slate-200">
          {chain.cadenaEmisores.map((issuer, index) => (
            <li key={`${issuer}-${index}`} className="flex items-start gap-2">
              <span className="mt-0.5 text-xs text-slate-400">{index + 1}.</span>
              <span>{issuer}</span>
            </li>
          ))}
        </ol>
      ) : (
        <p className="text-sm text-slate-500 dark:text-slate-400">Sin certificados en la cadena.</p>
      )}
      {chain.motivo && <p className="pt-1 text-xs text-slate-500 dark:text-slate-400">{chain.motivo}</p>}
    </InfoCard>
  )
}
