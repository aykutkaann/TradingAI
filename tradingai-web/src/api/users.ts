import { api } from './client'
import type { PublicProfileDto, UserStatsDto } from '@/types/profile'
import type { UserDto } from '@/types/auth'

export const usersApi = {
  getProfile: async (userId: string): Promise<PublicProfileDto> => {
    const response = await api.get<PublicProfileDto>(`/api/users/${userId}`)
    return response.data
  },

  getStats: async (userId: string): Promise<UserStatsDto> => {
    const response = await api.get<UserStatsDto>(`/api/users/${userId}/stats`)
    return response.data
  },

  follow: async (userId: string): Promise<void> => {
    await api.post(`/api/users/${userId}/follow`)
  },

  unfollow: async (userId: string): Promise<void> => {
    await api.delete(`/api/users/${userId}/follow`)
  },

  // ----- Self-edit endpoints (Settings page) ---------------------------
  updateMe: async (input: { displayName?: string; bio?: string }): Promise<UserDto> => {
    const response = await api.put<UserDto>('/api/users/me', input)
    return response.data
  },

  uploadAvatar: async (file: File): Promise<UserDto> => {
    const fd = new FormData()
    fd.append('File', file)
    const response = await api.post<UserDto>('/api/users/me/avatar', fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data
  },

  changePassword: async (input: {
    currentPassword: string
    newPassword: string
  }): Promise<void> => {
    await api.post('/api/auth/change-password', input)
  },
}
