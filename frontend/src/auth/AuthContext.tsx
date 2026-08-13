import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react'
import { ApiError } from '../api/client'
import * as authApi from '../api/authClient'
import type { UserDto } from '../api/types'

interface Session {
  accessToken: string
  refreshToken: string
  user: UserDto
}

interface AuthContextValue {
  user: UserDto | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  /**
   * Ejecuta una llamada autenticada (una función de api/authClient que recibe el access token).
   * Si el token venció, reintenta una vez luego de renovarlo con el refresh token; si el
   * refresh también falla, cierra la sesión local.
   */
  callWithAuth: <T>(fn: (accessToken: string) => Promise<T>) => Promise<T>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  // Deliberadamente en memoria, no en localStorage/cookies: recargar la página cierra la
  // sesión. Es un costo aceptado a cambio de no dejar tokens expuestos a un XSS persistente.
  const [session, setSession] = useState<Session | null>(null)

  const login = useCallback(async (email: string, password: string) => {
    const result = await authApi.login(email, password)
    setSession({ accessToken: result.accessToken, refreshToken: result.refreshToken, user: result.usuario })
  }, [])

  const logout = useCallback(() => {
    if (session) void authApi.logout(session.refreshToken)
    setSession(null)
  }, [session])

  const callWithAuth = useCallback(
    async <T,>(fn: (accessToken: string) => Promise<T>): Promise<T> => {
      if (!session) throw new ApiError('No hay una sesión activa.', 401)

      try {
        return await fn(session.accessToken)
      } catch (error) {
        if (!(error instanceof ApiError) || error.status !== 401) throw error

        try {
          const refreshed = await authApi.refresh(session.refreshToken)
          setSession({
            accessToken: refreshed.accessToken,
            refreshToken: refreshed.refreshToken,
            user: refreshed.usuario,
          })
          return await fn(refreshed.accessToken)
        } catch {
          setSession(null)
          throw new ApiError('La sesión expiró. Iniciá sesión nuevamente.', 401)
        }
      }
    },
    [session],
  )

  const value = useMemo<AuthContextValue>(
    () => ({ user: session?.user ?? null, isAuthenticated: session !== null, login, logout, callWithAuth }),
    [session, login, logout, callWithAuth],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth debe usarse dentro de un <AuthProvider>.')
  return context
}
