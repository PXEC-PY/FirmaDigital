import { useEffect, useState } from 'react'

const STAGES = [
  'Leyendo documento...',
  'Extrayendo firmas...',
  'Validando certificados...',
  'Consultando CRL...',
  'Verificando integridad...',
  'Comprobando cadena de confianza...',
  'Generando resultado...',
]

const STAGE_INTERVAL_MS = 900

export function LoadingStages() {
  const [stageIndex, setStageIndex] = useState(0)

  useEffect(() => {
    const timer = setInterval(() => {
      setStageIndex((current) => Math.min(current + 1, STAGES.length - 1))
    }, STAGE_INTERVAL_MS)
    return () => clearInterval(timer)
  }, [])

  return (
    <div className="flex min-h-64 flex-col items-center justify-center gap-6 rounded-2xl border border-slate-200/80 bg-white p-10 shadow-sm dark:border-slate-800 dark:bg-slate-900">
      <div className="relative flex h-12 w-12 items-center justify-center">
        <span className="absolute h-12 w-12 animate-spin rounded-full border-2 border-slate-200 border-t-blue-600 dark:border-slate-700 dark:border-t-blue-400" />
      </div>

      <div className="text-center">
        <p className="text-sm font-medium text-slate-900 dark:text-slate-100">{STAGES[stageIndex]}</p>
        <div className="mt-4 flex justify-center gap-1.5">
          {STAGES.map((stage, index) => (
            <span
              key={stage}
              className={`h-1.5 w-6 rounded-full transition-colors ${
                index <= stageIndex ? 'bg-blue-600 dark:bg-blue-400' : 'bg-slate-200 dark:bg-slate-700'
              }`}
            />
          ))}
        </div>
      </div>
    </div>
  )
}
