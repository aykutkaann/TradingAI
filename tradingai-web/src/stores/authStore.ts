import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { UserDto } from '@/types/auth'

type AuthState = {
  user: UserDto | null
  accessToken: string | null
  isAuthenticated: boolean
  setAuth: (user: UserDto, accessToken: string) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      isAuthenticated: false,

      setAuth: (user, accessToken) => {
        localStorage.setItem('accessToken', accessToken)
        set({ user, accessToken, isAuthenticated: true })
      },

      logout: () => {
        localStorage.removeItem('accessToken')
        set({ user: null, accessToken: null, isAuthenticated: false })
      },
    }),
    {
      name: 'auth-storage', // localStorage key for non-token state (user info)
      partialize: (state) => ({ user: state.user, isAuthenticated: state.isAuthenticated }),
    }
  )
)