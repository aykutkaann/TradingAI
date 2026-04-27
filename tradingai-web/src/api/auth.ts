import { api } from './client'
import type { AuthResponse, LoginRequest, RegisterRequest } from '@/types/auth'

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/api/auth/login', data)
    return response.data
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    try {
      const response = await api.post<AuthResponse>('/api/auth/register', data)

      // Validate the response has required fields
      if (!response.data?.accessToken || !response.data?.refreshToken || !response.data?.user) {
        throw new Error('Invalid registration response: missing required fields')
      }

      return response.data
    } catch (error) {
      // Re-throw with additional context
      console.error('Registration API error:', error)
      throw error
    }
  },
}