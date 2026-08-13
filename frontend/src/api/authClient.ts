import { ApiError } from './client'
import type { AuthResultDto, ProblemDetails, UserDto, UserRole } from './types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5214/api/v1'

async function parseErrorOrThrow(response: Response): Promise<never> {
  const problem = (await response.json().catch(() => null)) as ProblemDetails | null
  throw new ApiError(
    problem?.detail ?? problem?.title ?? 'Ocurrió un error inesperado.',
    response.status,
    problem?.errors ?? [],
  )
}

export async function login(email: string, password: string): Promise<AuthResultDto> {
  const response = await fetch(`${API_BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (!response.ok) await parseErrorOrThrow(response)
  return (await response.json()) as AuthResultDto
}

export async function refresh(refreshToken: string): Promise<AuthResultDto> {
  const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  })
  if (!response.ok) await parseErrorOrThrow(response)
  return (await response.json()) as AuthResultDto
}

export async function logout(refreshToken: string): Promise<void> {
  await fetch(`${API_BASE_URL}/auth/logout`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  }).catch(() => undefined) // logout es best-effort: no bloquear el cierre de sesión local
}

export async function getUsers(accessToken: string): Promise<UserDto[]> {
  const response = await fetch(`${API_BASE_URL}/users`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) await parseErrorOrThrow(response)
  return (await response.json()) as UserDto[]
}

export interface CreateUserInput {
  email: string
  nombreCompleto: string
  password: string
  role: UserRole
}

export async function createUser(accessToken: string, input: CreateUserInput): Promise<UserDto> {
  const response = await fetch(`${API_BASE_URL}/users`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify(input),
  })
  if (!response.ok) await parseErrorOrThrow(response)
  return (await response.json()) as UserDto
}
