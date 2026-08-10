import type {
  CertificateStatus,
  ChainStatus,
  OverallStatus,
  RevocationStatus,
} from '../api/types'

export type Tone = 'success' | 'danger' | 'warning' | 'neutral'

interface ToneClasses {
  badge: string
  dot: string
  text: string
}

const toneClasses: Record<Tone, ToneClasses> = {
  success: {
    badge: 'bg-emerald-50 text-emerald-700 ring-emerald-600/20',
    dot: 'bg-emerald-500',
    text: 'text-emerald-700',
  },
  danger: {
    badge: 'bg-red-50 text-red-700 ring-red-600/20',
    dot: 'bg-red-500',
    text: 'text-red-700',
  },
  warning: {
    badge: 'bg-amber-50 text-amber-700 ring-amber-600/20',
    dot: 'bg-amber-500',
    text: 'text-amber-700',
  },
  neutral: {
    badge: 'bg-slate-100 text-slate-600 ring-slate-500/20',
    dot: 'bg-slate-400',
    text: 'text-slate-600',
  },
}

export function getToneClasses(tone: Tone): ToneClasses {
  return toneClasses[tone]
}

export function overallStatusTone(status: OverallStatus): Tone {
  switch (status) {
    case 'Valido':
      return 'success'
    case 'Invalido':
      return 'danger'
    case 'Advertencia':
      return 'warning'
  }
}

export function overallStatusLabel(status: OverallStatus): string {
  switch (status) {
    case 'Valido':
      return 'Válido'
    case 'Invalido':
      return 'Inválido'
    case 'Advertencia':
      return 'Advertencia'
  }
}

export function certificateStatusTone(status: CertificateStatus): Tone {
  switch (status) {
    case 'Vigente':
      return 'success'
    case 'Revocado':
    case 'Expirado':
      return 'danger'
    case 'Desconocido':
      return 'neutral'
  }
}

export function certificateStatusLabel(status: CertificateStatus): string {
  switch (status) {
    case 'Vigente':
      return 'Vigente'
    case 'Revocado':
      return 'Revocado'
    case 'Expirado':
      return 'Expirado'
    case 'Desconocido':
      return 'Desconocido'
  }
}

export function chainStatusTone(status: ChainStatus): Tone {
  switch (status) {
    case 'Correcta':
      return 'success'
    case 'Incorrecta':
      return 'danger'
    case 'NoVerificable':
      return 'warning'
  }
}

export function chainStatusLabel(status: ChainStatus): string {
  switch (status) {
    case 'Correcta':
      return 'Correcta'
    case 'Incorrecta':
      return 'Incorrecta'
    case 'NoVerificable':
      return 'No verificable'
  }
}

export function revocationStatusTone(status: RevocationStatus): Tone {
  switch (status) {
    case 'NoRevocado':
      return 'success'
    case 'Revocado':
      return 'danger'
    case 'NoVerificable':
      return 'warning'
  }
}

export function revocationStatusLabel(status: RevocationStatus): string {
  switch (status) {
    case 'NoRevocado':
      return 'No revocado'
    case 'Revocado':
      return 'Revocado'
    case 'NoVerificable':
      return 'No verificable'
  }
}
