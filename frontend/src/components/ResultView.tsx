import type { DocumentValidationResponseDto } from '../api/types'
import { StatusBanner } from './StatusBanner'
import { DocumentCard } from './DocumentCard'
import { SignatureResultBlock } from './SignatureResultBlock'

interface ResultViewProps {
  result: DocumentValidationResponseDto
  onValidateAnother: () => void
}

export function ResultView({ result, onValidateAnother }: ResultViewProps) {
  return (
    <div className="space-y-6">
      <StatusBanner status={result.estadoGeneral} reason={result.motivo} fileName={result.nombreArchivo} />

      <DocumentCard
        fileName={result.nombreArchivo}
        hashSha256={result.hashSha256}
        integrity={result.documento}
      />

      <div className="space-y-8">
        {result.firmas.map((signature, index) => (
          <SignatureResultBlock
            key={signature.nombreCampo}
            signature={signature}
            index={index}
            total={result.firmas.length}
          />
        ))}
      </div>

      <div className="flex justify-center pt-2">
        <button
          type="button"
          onClick={onValidateAnother}
          className="rounded-full border border-slate-300 bg-white px-5 py-2.5 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-200 dark:hover:bg-slate-700"
        >
          Validar otro documento
        </button>
      </div>
    </div>
  )
}
