import { Route, Routes } from 'react-router-dom'
import { Header } from './components/Header'
import { ProtectedRoute } from './components/ProtectedRoute'
import { ValidatorPage } from './pages/ValidatorPage'
import { LoginPage } from './pages/LoginPage'
import { UsersPage } from './pages/UsersPage'

function App() {
  return (
    <div className="min-h-full" style={{ backgroundColor: 'var(--meridional-teal)' }}>
      <Header />

      <Routes>
        <Route path="/" element={<ValidatorPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/admin/usuarios"
          element={
            <ProtectedRoute allowedRoles={['Administrador']}>
              <UsersPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </div>
  )
}

export default App
