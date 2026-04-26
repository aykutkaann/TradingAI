import axios, { AxiosError, type AxiosRequestConfig } from 'axios'
import type { AuthResponse } from '@/types/auth'

const baseURL = import.meta.env.VITE_API_URL || 'https://localhost:7155'

export const api = axios.create({ baseURL })

// Attach access token to every request automatically.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// ---------------------------------------------------------------------------
// 401 → refresh-and-retry flow.
//
// When the access token expires, the backend returns 401. Instead of
// immediately logging the user out, we:
//   1. Call POST /api/auth/refresh-token with the stored refresh token
//   2. If the refresh succeeds → store the new tokens and retry the
//      original request transparently.
//   3. If the refresh fails (or there's no refresh token) → wipe auth
//      state and redirect to /login.
//
// Concurrency: if multiple requests fire 401 simultaneously, we only want
// to call /refresh-token ONCE. We share a single in-flight promise so all
// pending requests await the same refresh, then retry with the new token.
// ---------------------------------------------------------------------------

let refreshPromise: Promise<string> | null = null

async function refreshAccessToken(): Promise<string> {
  const refreshToken = localStorage.getItem('refreshToken')
  if (!refreshToken) throw new Error('No refresh token available')

  // Use a bare axios call (NOT the `api` instance) to avoid recursion
  // through the interceptors.
  const response = await axios.post<AuthResponse>(
    `${baseURL}/api/auth/refresh-token`,
    { token: refreshToken, ipAddress: '' }
  )

  const { accessToken, refreshToken: newRefreshToken } = response.data

  // Update both localStorage and the Zustand store so React components see it.
  // Lazy-import to avoid a circular dependency between client.ts and authStore.
  const { useAuthStore } = await import('@/stores/authStore')
  useAuthStore.getState().setTokens(accessToken, newRefreshToken)

  return accessToken
}

function forceLogout() {
  localStorage.removeItem('accessToken')
  localStorage.removeItem('refreshToken')
  // Best-effort store reset (lazy import to avoid circular deps at module load)
  import('@/stores/authStore').then(({ useAuthStore }) => useAuthStore.getState().logout())
  if (window.location.pathname !== '/login') {
    window.location.href = '/login'
  }
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as AxiosRequestConfig & { _retry?: boolean }

    // Only handle 401s that haven't been retried yet, and never retry
    // the refresh endpoint itself (would loop forever).
    const isAuthEndpoint = original?.url?.includes('/api/auth/refresh-token')
    if (error.response?.status !== 401 || original?._retry || isAuthEndpoint) {
      return Promise.reject(error)
    }

    original._retry = true

    try {
      // Share a single in-flight refresh across all concurrent 401s.
      if (!refreshPromise) {
        refreshPromise = refreshAccessToken().finally(() => {
          refreshPromise = null
        })
      }
      const newAccessToken = await refreshPromise

      // Retry the original request with the fresh token.
      original.headers = { ...original.headers, Authorization: `Bearer ${newAccessToken}` }
      return api.request(original)
    } catch (refreshErr) {
      forceLogout()
      return Promise.reject(refreshErr)
    }
  }
)
