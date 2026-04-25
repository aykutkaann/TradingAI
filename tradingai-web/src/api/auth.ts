import { api } from './client'
import type { AuthResponse, LoginRequest, RegisterRequest } from '@/types/auth'

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/api/auth/login', data)
    return response.data
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/api/auth/register', data)
    return response.data
  },
}