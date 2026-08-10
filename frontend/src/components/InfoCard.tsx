import type { ReactNode } from 'react'

interface InfoCardProps {
  title: string
  icon?: ReactNode
  headerRight?: ReactNode
  children: ReactNode
}

export function InfoCard({ title, icon, headerRight, children }: InfoCardProps) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="mb-4 flex items-center justify-between gap-2">
        <h3 className="flex items-center gap-2 text-sm font-semibold text-slate-900">
          {icon}
          {title}
        </h3>
        {headerRight}
      </div>
      <div className="space-y-2.5">{children}</div>
    </div>
  )
}

interface InfoRowProps {
  label: string
  value: ReactNode
}

export function InfoRow({ label, value }: InfoRowProps) {
  return (
    <div className="flex items-start justify-between gap-4 text-sm">
      <span className="shrink-0 text-slate-500">{label}</span>
      <span className="text-right font-medium text-slate-900">{value}</span>
    </div>
  )
}
