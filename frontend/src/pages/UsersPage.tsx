import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useAuth } from '../auth/AuthContext'
import * as authApi from '../api/authClient'
import type { UserDto } from '../api/types'
import { ApiError } from '../api/client'
import { formatDateTime } from '../lib/format'
import { createUserSchema, type CreateUserFormValues } from '../schemas/createUserSchema'
import { StatusBadge } from '../components/StatusBadge'

export function UsersPage() {
  const { callWithAuth, user } = useAuth()
  const [users, setUsers] = useState<UserDto[]>([])
  const [loadError, setLoadError] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateUserFormValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { role: 'Usuario' },
  })

  async function loadUsers() {
    try {
      const result = await callWithAuth((token) => authApi.getUsers(token))
      setUsers(result)
      setLoadError(null)
    } catch (error) {
      setLoadError(error instanceof ApiError ? error.message : 'No se pudo cargar la lista de usuarios.')
    }
  }

  useEffect(() => {
    void loadUsers()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function onSubmit(values: CreateUserFormValues) {
    setFormError(null)
    setIsSubmitting(true)
    try {
      await callWithAuth((token) => authApi.createUser(token, values))
      reset({ email: '', nombreCompleto: '', password: '', role: 'Usuario' })
      await loadUsers()
    } catch (error) {
      setFormError(error instanceof ApiError ? error.message : 'No se pudo crear el usuario.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6 px-6 py-10">
      <div>
        <h1 className="text-lg font-semibold text-slate-100">Usuarios</h1>
        <p className="text-sm text-slate-300">Sesión iniciada como {user?.email} ({user?.role}).</p>
      </div>

      <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-lg">
        <h2 className="mb-4 text-sm font-semibold text-slate-900">Crear usuario</h2>
        <form onSubmit={handleSubmit(onSubmit)} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="block text-sm font-medium text-slate-700">Email</label>
            <input
              type="email"
              {...register('email')}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            />
            {errors.email && <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700">Nombre completo</label>
            <input
              type="text"
              {...register('nombreCompleto')}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            />
            {errors.nombreCompleto && (
              <p className="mt-1 text-xs text-red-600">{errors.nombreCompleto.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700">Contraseña inicial</label>
            <input
              type="password"
              {...register('password')}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            />
            {errors.password && <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700">Rol</label>
            <select
              {...register('role')}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
            >
              <option value="Usuario">Usuario</option>
              <option value="Auditor">Auditor</option>
              <option value="Administrador">Administrador</option>
            </select>
          </div>

          {formError && (
            <div className="sm:col-span-2 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
              {formError}
            </div>
          )}

          <div className="sm:col-span-2">
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-full px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition disabled:cursor-not-allowed disabled:opacity-60"
              style={{ backgroundColor: 'var(--meridional-teal)' }}
            >
              {isSubmitting ? 'Creando...' : 'Crear usuario'}
            </button>
          </div>
        </form>
      </div>

      <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-lg">
        <h2 className="mb-4 text-sm font-semibold text-slate-900">Usuarios existentes</h2>
        {loadError && <p className="text-sm text-red-600">{loadError}</p>}
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-200 text-slate-500">
                <th className="py-2 pr-4 font-medium">Email</th>
                <th className="py-2 pr-4 font-medium">Nombre</th>
                <th className="py-2 pr-4 font-medium">Rol</th>
                <th className="py-2 pr-4 font-medium">Estado</th>
                <th className="py-2 pr-4 font-medium">Último acceso</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u) => (
                <tr key={u.id} className="border-b border-slate-100 text-slate-800">
                  <td className="py-2 pr-4">{u.email}</td>
                  <td className="py-2 pr-4">{u.nombreCompleto}</td>
                  <td className="py-2 pr-4">{u.role}</td>
                  <td className="py-2 pr-4">
                    <StatusBadge label={u.activo ? 'Activo' : 'Inactivo'} tone={u.activo ? 'success' : 'neutral'} />
                  </td>
                  <td className="py-2 pr-4">{formatDateTime(u.ultimoAccesoUtc)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
