import { api } from './client'
import type { WatchlistItemDto } from '@/types/analysis'

export const watchlistApi = {
  list: async (): Promise<WatchlistItemDto[]> => {
    const response = await api.get<WatchlistItemDto[]>('/api/watchlist')
    return response.data
  },

  add: async (assetId: string): Promise<{ id: string }> => {
    const response = await api.post<{ id: string }>('/api/watchlist', { assetId })
    return response.data
  },

  remove: async (assetId: string): Promise<void> => {
    await api.delete(`/api/watchlist/${assetId}`)
  },
}
