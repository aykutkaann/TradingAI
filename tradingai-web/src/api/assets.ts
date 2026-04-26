import { api } from './client'
import type { AssetDto, AssetType } from '@/types/analysis'

export const assetsApi = {
  list: async (type?: AssetType): Promise<AssetDto[]> => {
    const response = await api.get<AssetDto[]>('/api/assets', {
      params: type !== undefined ? { type } : undefined,
    })
    return response.data
  },
}
