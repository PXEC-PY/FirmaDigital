import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function Header() {
  const { isAuthenticated, user, logout } = useAuth()

  return (
    <header className="border-b border-slate-200 bg-white">
      <div className="mx-auto flex max-w-5xl items-center justify-between gap-3 px-6 py-5">
        <Link to="/" className="flex items-center gap-3">
          <img
            src={`${import.meta.env.BASE_URL}logo.png`}
            alt=""
            className="h-10 w-10 rounded-lg object-contain"
          />
          <div>
            <h1 className="text-base font-semibold tracking-tight text-slate-900">
              Validador de Firmas Digitales del Paraguay
            </h1>
            <p className="text-sm text-slate-500">
              Verificación de integridad, cadena de confianza y revocación sobre la PKI paraguaya.
            </p>
          </div>
        </Link>

        <nav className="flex shrink-0 items-center gap-4 text-sm">
          {isAuthenticated ? (
            <>
              {user?.role === 'Administrador' && (
                <Link to="/admin/usuarios" className="font-medium text-slate-600 hover:text-slate-900">
                  Usuarios
                </Link>
              )}
              <span className="text-slate-400">{user?.email}</span>
              <button
                type="button"
                onClick={logout}
                className="font-medium text-slate-600 hover:text-slate-900"
              >
                Cerrar sesión
              </button>
            </>
          ) : (
            <Link to="/login" className="font-medium text-slate-600 hover:text-slate-900">
              Iniciar sesión
            </Link>
          )}
        </nav>
      </div>
    </header>
  )
}
