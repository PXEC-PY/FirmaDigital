import type { OverallStatus } from '../api/types'

interface StatusBannerProps {
  status: OverallStatus
  reason: string
  fileName: string
}

const ICONS: Record<OverallStatus, string> = {
  Valido: '🟢',
  Invalido: '🔴',
  Advertencia: '🟡',
}

const TITLES: Record<OverallStatus, string> = {
  Valido: 'DOCUMENTO VÁLIDO',
  Invalido: 'DOCUMENTO INVÁLIDO',
  Advertencia: 'DOCUMENTO CON ADVERTENCIAS',
}

const CONTAINER_CLASSES: Record<OverallStatus, string> = {
  Valido: 'border-emerald-200 bg-emerald-50 dark:border-emerald-500/30 dark:bg-emerald-500/10',
  Invalido: 'border-red-200 bg-red-50 dark:border-red-500/30 dark:bg-red-500/10',
  Advertencia: 'border-amber-200 bg-amber-50 dark:border-amber-500/30 dark:bg-amber-500/10',
}

export function StatusBanner({ status, reason, fileName }: StatusBannerProps) {
  return (
    <div
      role="status"
      className={`rounded-2xl border p-6 shadow-sm sm:p-8 ${CONTAINER_CLASSES[status]}`}
    >
      <div className="flex flex-col items-start gap-4 sm:flex-row sm:items-center">
        <span className="text-5xl leading-none" aria-hidden>
          {ICONS[status]}
        </span>
        <div>
          <h2 className="text-xl font-bold tracking-tight text-slate-900 dark:text-slate-50">
            {TITLES[status]}
          </h2>
          <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">{reason}</p>
          <p className="mt-2 truncate text-xs text-slate-400 dark:text-slate-500">{fileName}</p>
        </div>
      </div>
    </div>
  )
}
