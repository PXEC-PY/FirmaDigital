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
    <div className="mx-auto flex min-h-64 max-w-xl flex-col items-center justify-center gap-6 rounded-2xl border border-slate-200 bg-white p-10 shadow-lg">
      <div className="relative flex h-12 w-12 items-center justify-center">
        <span
          className="absolute h-12 w-12 animate-spin rounded-full border-2 border-slate-200"
          style={{ borderTopColor: 'var(--meridional-teal)' }}
        />
      </div>

      <div className="text-center">
        <p className="text-sm font-medium text-slate-900">{STAGES[stageIndex]}</p>
        <div className="mt-4 flex justify-center gap-1.5">
          {STAGES.map((stage, index) => (
            <span
              key={stage}
              className="h-1.5 w-6 rounded-full transition-colors"
              style={{
                backgroundColor: index <= stageIndex ? 'var(--meridional-teal)' : '#e2e8f0',
              }}
            />
          ))}
        </div>
      </div>
    </div>
  )
}
