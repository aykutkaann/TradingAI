import { useParams, useNavigate, Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { ArrowLeft, Trash2, Globe, Lock } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { analysisApi } from '@/api/analysis'
import { AnalysisResultCard } from '@/components/analysis/AnalysisResultCard'
import { LikeButton } from '@/components/analysis/LikeButton'
import { CommentsSection } from '@/components/analysis/CommentsSection'
import { useApiErrorToast } from '@/lib/toastError'
import { useAuthStore } from '@/stores/authStore'

export function AnalysisDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const currentUserId = useAuthStore((s) => s.user?.id)
  const toastError = useApiErrorToast()

  const { data, isLoading, isError } = useQuery({
    queryKey: ['analysis', id],
    queryFn: () => analysisApi.getById(id!),
    enabled: !!id,
  })

  const deleteMutation = useMutation({
    mutationFn: () => analysisApi.delete(id!),
    onSuccess: () => {
      toast.success('Analysis deleted')
      queryClient.invalidateQueries({ queryKey: ['my-analyses'] })
      navigate('/analyses')
    },
    onError: () => toast.error('Failed to delete'),
  })

  const publishMutation = useMutation({
    mutationFn: () => (data?.isPublished ? analysisApi.unpublish(id!) : analysisApi.publish(id!)),
    onSuccess: () => {
      toast.success(data?.isPublished ? 'Unpublished' : 'Published to feed')
      queryClient.invalidateQueries({ queryKey: ['analysis', id] })
    },
    onError: (err) => toastError(err, 'Failed to update visibility'),
  })

  if (isLoading) return <Skeleton className="h-[600px] w-full max-w-4xl mx-auto" />
  if (isError || !data) {
    return (
      <div className="max-w-2xl mx-auto text-center py-16">
        <p className="text-destructive mb-4">Analysis not found.</p>
        <Button asChild variant="outline">
          <Link to="/analyses">Back to my analyses</Link>
        </Button>
      </div>
    )
  }

  const isOwner = data.userId === currentUserId

  return (
    <div className="max-w-4xl mx-auto space-y-4">
      <div className="flex items-center justify-between gap-2 flex-wrap">
        <Button variant="ghost" size="sm" asChild>
          <Link to="/analyses">
            <ArrowLeft className="size-4 mr-1" /> Back
          </Link>
        </Button>

        <div className="flex gap-2 flex-wrap">
          <LikeButton analysis={data} />

          {isOwner && (
            <>
              <Button
                variant="outline"
                size="sm"
                onClick={() => publishMutation.mutate()}
                disabled={publishMutation.isPending}
              >
                {data.isPublished ? (
                  <>
                    <Lock className="size-4 mr-1" /> Unpublish
                  </>
                ) : (
                  <>
                    <Globe className="size-4 mr-1" /> Publish
                  </>
                )}
              </Button>
              <Button
                variant="destructive"
                size="sm"
                onClick={() => {
                  if (confirm('Delete this analysis?')) deleteMutation.mutate()
                }}
                disabled={deleteMutation.isPending}
              >
                <Trash2 className="size-4 mr-1" /> Delete
              </Button>
            </>
          )}
        </div>
      </div>

      <AnalysisResultCard analysis={data} />

      <CommentsSection analysisId={data.id} />
    </div>
  )
}
