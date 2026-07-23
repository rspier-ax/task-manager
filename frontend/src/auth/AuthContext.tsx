import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import * as authApi from '../api/authApi'
import type { AuthResponse } from '../types'

const TOKEN_KEY = 'taskmanager.token'
const USER_KEY = 'taskmanager.user'

type AuthUser = {
  userId: string
  email: string
  displayName: string
}

type AuthContextValue = {
  token: string | null
  user: AuthUser | null
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string, displayName: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function readStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as AuthUser
  } catch {
    return null
  }
}

function persist(auth: AuthResponse) {
  localStorage.setItem(TOKEN_KEY, auth.token)
  localStorage.setItem(
    USER_KEY,
    JSON.stringify({
      userId: auth.userId,
      email: auth.email,
      displayName: auth.displayName,
    }),
  )
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY))
  const [user, setUser] = useState<AuthUser | null>(() => readStoredUser())

  const applyAuth = useCallback((auth: AuthResponse) => {
    persist(auth)
    setToken(auth.token)
    setUser({
      userId: auth.userId,
      email: auth.email,
      displayName: auth.displayName,
    })
  }, [])

  const login = useCallback(
    async (email: string, password: string) => {
      const auth = await authApi.login(email, password)
      applyAuth(auth)
    },
    [applyAuth],
  )

  const register = useCallback(
    async (email: string, password: string, displayName: string) => {
      const auth = await authApi.register(email, password, displayName)
      applyAuth(auth)
    },
    [applyAuth],
  )

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    setToken(null)
    setUser(null)
  }, [])

  const value = useMemo(
    () => ({ token, user, login, register, logout }),
    [token, user, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return ctx
}
