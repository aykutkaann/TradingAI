import { api } from './client'
import type { CommentDto } from '@/types/comment'
import type { PagedResult } from '@/types/analysis'

export const commentsApi = {
  list: async (
    analysisId: string,
    page = 1,
    pageSize = 50
  ): Promise<PagedResult<CommentDto>> => {
    const response = await api.get<PagedResult<CommentDto>>(
      `/api/analysis/${analysisId}/comments`,
      { params: { page, pageSize } }
    )
    return response.data
  },

  create: async (analysisId: string, content: string): Promise<CommentDto> => {
    const response = await api.post<CommentDto>(
      `/api/analysis/${analysisId}/comments`,
      { content }
    )
    return response.data
  },

  delete: async (commentId: string): Promise<void> => {
    await api.delete(`/api/analysis/comments/${commentId}`)
  },
}
