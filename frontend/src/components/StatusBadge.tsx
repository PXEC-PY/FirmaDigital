import { getToneClasses, type Tone } from '../lib/statusStyles'

interface StatusBadgeProps {
  label: string
  tone: Tone
}

export function StatusBadge({ label, tone }: StatusBadgeProps) {
  const classes = getToneClasses(tone)
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset ${classes.badge}`}
    >
      <span className={`h-1.5 w-1.5 rounded-full ${classes.dot}`} />
      {label}
    </span>
  )
}
