import type { SignerDto } from '../api/types'
import { InfoCard, InfoRow } from './InfoCard'

export function SignerCard({ signer }: { signer: SignerDto }) {
  return (
    <InfoCard title="Firmante">
      <InfoRow label="Nombre completo" value={signer.nombreCompleto} />
      <InfoRow label="Número de documento" value={signer.numeroDocumento ?? 'No disponible'} />
      <InfoRow label="Correo" value={signer.correo ?? 'No disponible'} />
      <InfoRow label="Empresa" value={signer.empresa ?? 'No disponible'} />
      <InfoRow label="Cargo" value={signer.cargo ?? 'No disponible'} />
    </InfoCard>
  )
}
