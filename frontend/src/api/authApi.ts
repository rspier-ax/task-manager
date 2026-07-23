import { apiRequest } from './client'
import type { AuthResponse } from '../types'

export function login(email: string, password: string): Promise<AuthResponse> {
  return apiRequest<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: { email, password },
  })
}

export function register(
  email: string,
  password: string,
  displayName: string,
): Promise<AuthResponse> {
  return apiRequest<AuthResponse>('/api/auth/register', {
    method: 'POST',
    body: { email, password, displayName },
  })
}
