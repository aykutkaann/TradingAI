import { api } from './client'
import type {
  AnalysisDto,
  AnalyzeAssetRequest,
  AnalyzeImageInput,
  PagedResult,
} from '@/types/analysis'

export const analysisApi = {
  analyzeImage: async (input: AnalyzeImageInput): Promise<AnalysisDto> => {
    const formData = new FormData()
    formData.append('File', input.file)
    formData.append('AssetPair', input.assetPair)
    formData.append('TimeFrame', input.timeFrame)
    if (input.userPrompt) formData.append('UserPrompt', input.userPrompt)

    const response = await api.post<AnalysisDto>('/api/analysis/image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data
  },

  analyzeAsset: async (req: AnalyzeAssetRequest): Promise<AnalysisDto> => {
    const response = await api.post<AnalysisDto>('/api/analysis/assets', req)
    return response.data
  },

  getMine: async (page = 1, pageSize = 20): Promise<PagedResult<AnalysisDto>> => {
    const response = await api.get<PagedResult<AnalysisDto>>('/api/analysis', {
      params: { page, pageSize },
    })
    return response.data
  },

  getPublicFeed: async (page = 1, pageSize = 20): Promise<PagedResult<AnalysisDto>> => {
    const response = await api.get<PagedResult<AnalysisDto>>('/api/analysis/feed', {
      params: { page, pageSize },
    })
    return response.data
  },

  getFollowingFeed: async (page = 1, pageSize = 20): Promise<PagedResult<AnalysisDto>> => {
    const response = await api.get<PagedResult<AnalysisDto>>('/api/analysis/following-feed', {
      params: { page, pageSize },
    })
    return response.data
  },

  getById: async (id: string): Promise<AnalysisDto> => {
    const response = await api.get<AnalysisDto>(`/api/analysis/${id}`)
    return response.data
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/api/analysis/${id}`)
  },

  publish: async (id: string): Promise<void> => {
    await api.put(`/api/analysis/${id}/publish`)
  },

  unpublish: async (id: string): Promise<void> => {
    await api.put(`/api/analysis/${id}/unpublish`)
  },

  like: async (id: string): Promise<void> => {
    await api.post(`/api/analysis/${id}/like`)
  },

  unlike: async (id: string): Promise<void> => {
    await api.delete(`/api/analysis/${id}/like`)
  },
}
