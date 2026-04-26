import { api } from './client'
import type { NotificationDto } from '@/types/notification'
import type { PagedResult } from '@/types/analysis'

export const notificationsApi = {
  list: async (page = 1, pageSize = 20, unreadOnly?: boolean): Promise<PagedResult<NotificationDto>> => {
    const response = await api.get<PagedResult<NotificationDto>>('/api/notifications', {
      params: { page, pageSize, unreadOnly },
    })
    return response.data
  },

  unreadCount: async (): Promise<number> => {
    const response = await api.get<number>('/api/notifications/unread-count')
    return response.data
  },

  markAsRead: async (id: string): Promise<void> => {
    await api.post(`/api/notifications/${id}/read`)
  },

  markAllAsRead: async (): Promise<{ markedCount: number }> => {
    const response = await api.post<{ markedCount: number }>('/api/notifications/read-all')
    return response.data
  },
}
