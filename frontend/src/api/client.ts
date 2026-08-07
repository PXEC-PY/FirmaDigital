import type { DocumentValidationResponseDto, ProblemDetails } from './types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5214/api/v1'

export class ApiError extends Error {
  readonly status: number
  readonly errors: string[]

  constructor(message: string, status: number, errors: string[] = []) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.errors = errors
  }
}

export async function validateDocument(
  file: File,
  signal?: AbortSignal,
): Promise<DocumentValidationResponseDto> {
  const formData = new FormData()
  formData.append('file', file)

  const response = await fetch(`${API_BASE_URL}/validations`, {
    method: 'POST',
    body: formData,
    signal,
  })

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null
    throw new ApiError(
      problem?.detail ?? problem?.title ?? 'No se pudo validar el documento.',
      response.status,
      problem?.errors ?? [],
    )
  }

  return (await response.json()) as DocumentValidationResponseDto
}
