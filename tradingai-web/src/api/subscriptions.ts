import { api } from './client'
import type { SubscriptionDto, SubscriptionTier } from '@/types/subscription'

export const subscriptionsApi = {
  getMine: async (): Promise<SubscriptionDto> => {
    const response = await api.get<SubscriptionDto>('/api/subscription/me')
    return response.data
  },

  upgrade: async (newTier: SubscriptionTier): Promise<SubscriptionDto> => {
    const response = await api.post<SubscriptionDto>('/api/subscription/upgrade', { newTier })
    return response.data
  },

  cancel: async (): Promise<SubscriptionDto> => {
    const response = await api.post<SubscriptionDto>('/api/subscription/cancel')
    return response.data
  },
}
