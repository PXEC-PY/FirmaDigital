import type { SignatureDto } from '../api/types'
import { SignerCard } from './SignerCard'
import { SignatureCard } from './SignatureCard'
import { CertificateCard } from './CertificateCard'
import { ChainCard } from './ChainCard'
import { RevocationCard } from './RevocationCard'
import { TimestampCard } from './TimestampCard'

interface SignatureResultBlockProps {
  signature: SignatureDto
  index: number
  total: number
}

export function SignatureResultBlock({ signature, index, total }: SignatureResultBlockProps) {
  return (
    <section>
      {total > 1 && (
        <h4 className="mb-3 text-xs font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">
          Firma {index + 1} de {total} · {signature.nombreCampo}
        </h4>
      )}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
        <SignerCard signer={signature.firmante} />
        <SignatureCard signature={signature} />
        <CertificateCard certificate={signature.certificado} />
        <ChainCard chain={signature.certificado.cadena} />
        <RevocationCard revocation={signature.certificado.revocacion} />
        <TimestampCard timestamp={signature.timestamp} />
      </div>
    </section>
  )
}
