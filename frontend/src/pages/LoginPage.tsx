import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { SquareCheckBig } from 'lucide-react'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/AuthContext'

export function LoginPage() {
  const { token, login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('demo@taskmanager.local')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  if (token) {
    return <Navigate to="/" replace />
  }

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await login(email.trim(), password)
      navigate('/', { replace: true })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Login failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="auth-shell">
      <section className="card auth-card">
        <div className="auth-brand">
          <span className="brand-mark" aria-hidden>
            <SquareCheckBig size={28} strokeWidth={2.25} />
          </span>
          <p className="brand-name">TaskManager</p>
        </div>
        <h1 className="auth-title">Sign in</h1>
        <p className="muted auth-lead">Manage titles, status, and due dates in one place.</p>
        <form onSubmit={onSubmit} className="stack">
          <label>
            Email
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="username"
            />
          </label>
          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              autoComplete="current-password"
            />
          </label>
          {error ? <p className="error">{error}</p> : null}
          <button type="submit" className="btn-block" disabled={loading}>
            {loading ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
        <p className="muted demo-line">
          Demo: <code>demo@taskmanager.local</code> / <code>Demo123!</code>
        </p>
        <p className="muted footer-note">
          No account? <Link to="/register">Create one</Link>
        </p>
      </section>
    </main>
  )
}
