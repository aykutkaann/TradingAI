import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { Heart } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { analysisApi } from '@/api/analysis'
import { useAuthStore } from '@/stores/authStore'
import type { AnalysisDto } from '@/types/analysis'
import { getApiErrorMessage } from '@/lib/errors'

type Props = {
  analysis: Pick<AnalysisDto, 'id' | 'isLikedByMe' | 'likeCount'>
}

/**
 * Like/unlike toggle with optimistic update.
 *
 * The pattern: when the user clicks, we *immediately* mutate the React Query
 * cache to flip the heart and adjust the count, BEFORE the network call
 * completes. The UI feels instant. If the call fails, we roll back.
 *
 * C# parallel: this is essentially `try { applyChange(); await save(); } catch { revertChange(); }`
 * but for the UI cache instead of an in-memory entity.
 */
export function LikeButton({ analysis }: Props) {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  const queryKey = ['analysis', analysis.id]

  const mutation = useMutation({
    mutationFn: () =>
      analysis.isLikedByMe ? analysisApi.unlike(analysis.id) : analysisApi.like(analysis.id),

    // Run BEFORE the network call: optimistically update the cache.
    onMutate: async () => {
      // Cancel any in-flight refetch so it doesn't overwrite our optimistic value.
      await queryClient.cancelQueries({ queryKey })

      const previous = queryClient.getQueryData<AnalysisDto>(queryKey)

      if (previous) {
        queryClient.setQueryData<AnalysisDto>(queryKey, {
          ...previous,
          isLikedByMe: !previous.isLikedByMe,
          likeCount: previous.likeCount + (previous.isLikedByMe ? -1 : 1),
        })
      }

      // Return a context object so onError can roll back.
      return { previous }
    },

    onError: (err, _vars, context) => {
      // Roll back to the snapshot we took in onMutate.
      if (context?.previous) {
        queryClient.setQueryData(queryKey, context.previous)
      }
      toast.error(getApiErrorMessage(err, 'Action failed'))
    },

    // Always refetch from server to make sure we're in sync (especially
    // the count, which can be off if other users liked concurrently).
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey })
    },
  })

  const handleClick = () => {
    if (!isAuthenticated) {
      toast.info('Sign in to like analyses')
      navigate('/login')
      return
    }
    mutation.mutate()
  }

  return (
    <Button
      variant={analysis.isLikedByMe ? 'default' : 'outline'}
      size="sm"
      onClick={handleClick}
      disabled={mutation.isPending}
    >
      <Heart
        className={`size-4 mr-1 ${analysis.isLikedByMe ? 'fill-current' : ''}`}
      />
      {analysis.likeCount}
    </Button>
  )
}
