import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { Trash2, Send } from 'lucide-react'
import { assetUrl } from '@/lib/assetUrl'

import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Textarea } from '@/components/ui/textarea'
import { Skeleton } from '@/components/ui/skeleton'
import { commentsApi } from '@/api/comments'
import { useAuthStore } from '@/stores/authStore'
import { getApiErrorMessage } from '@/lib/errors'
import type { CommentDto } from '@/types/comment'

type Props = {
  analysisId: string
}

export function CommentsSection({ analysisId }: Props) {
  const queryClient = useQueryClient()
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const currentUserId = useAuthStore((s) => s.user?.id)

  const queryKey = ['comments', analysisId]

  const { data, isLoading, isError } = useQuery({
    queryKey,
    queryFn: () => commentsApi.list(analysisId),
  })

  const createMutation = useMutation({
    mutationFn: (content: string) => commentsApi.create(analysisId, content),
    onSuccess: () => {
      // Refetch instead of optimistic insert — the server-generated id and
      // createdAt would otherwise be guesses, and the comment count on the
      // parent analysis card needs to refresh too.
      queryClient.invalidateQueries({ queryKey })
      queryClient.invalidateQueries({ queryKey: ['analysis', analysisId] })
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Failed to post comment')),
  })

  const deleteMutation = useMutation({
    mutationFn: (commentId: string) => commentsApi.delete(commentId),
    onSuccess: () => {
      toast.success('Comment deleted')
      queryClient.invalidateQueries({ queryKey })
      queryClient.invalidateQueries({ queryKey: ['analysis', analysisId] })
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Failed to delete')),
  })

  return (
    <Card>
      <CardContent className="p-6 space-y-6">
        <h2 className="text-xl font-semibold">
          Comments {data ? <span className="text-muted-foreground">({data.totalCount})</span> : null}
        </h2>

        {isAuthenticated ? (
          <CommentForm
            isPending={createMutation.isPending}
            onSubmit={(content) => createMutation.mutate(content)}
          />
        ) : (
          <SignInPrompt />
        )}

        {isLoading && (
          <div className="space-y-3">
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-16 w-full" />
          </div>
        )}

        {isError && <p className="text-destructive">Failed to load comments.</p>}

        {data && data.items.length === 0 && !isLoading && (
          <p className="text-sm text-muted-foreground py-4">
            No comments yet. Be the first to share your thoughts.
          </p>
        )}

        {data && data.items.length > 0 && (
          <div className="space-y-4">
            {data.items.map((c) => (
              <CommentItem
                key={c.id}
                comment={c}
                canDelete={c.userId === currentUserId}
                onDelete={() => deleteMutation.mutate(c.id)}
                isDeleting={deleteMutation.isPending && deleteMutation.variables === c.id}
              />
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  )
}

// ---------------------------------------------------------------------------
function SignInPrompt() {
  return (
    <div className="text-sm text-muted-foreground">
      <Link to="/login" className="text-primary hover:underline">
        Sign in
      </Link>{' '}
      to join the discussion.
    </div>
  )
}

// ---------------------------------------------------------------------------
function CommentForm({
  isPending,
  onSubmit,
}: {
  isPending: boolean
  onSubmit: (content: string) => void
}) {
  const [content, setContent] = useState('')

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    const trimmed = content.trim()
    if (!trimmed) return
    onSubmit(trimmed)
    setContent('')
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-2">
      <Textarea
        placeholder="Add a comment..."
        value={content}
        onChange={(e) => setContent(e.target.value)}
        rows={3}
        maxLength={1000}
      />
      <div className="flex justify-between items-center">
        <span className="text-xs text-muted-foreground">
          {content.length} / 1000
        </span>
        <Button type="submit" size="sm" disabled={isPending || !content.trim()}>
          <Send className="size-4 mr-1" />
          {isPending ? 'Posting...' : 'Post'}
        </Button>
      </div>
    </form>
  )
}

// ---------------------------------------------------------------------------
function CommentItem({
  comment,
  canDelete,
  onDelete,
  isDeleting,
}: {
  comment: CommentDto
  canDelete: boolean
  onDelete: () => void
  isDeleting: boolean
}) {
  const navigate = useNavigate()
  const initials = comment.userDisplayName.slice(0, 2).toUpperCase()

  return (
    <div className="flex gap-3 group">
      <Avatar
        className="size-8 cursor-pointer"
        onClick={() => navigate(`/users/${comment.userId}`)}
      >
        {comment.userAvatarUrl ? (
          <AvatarImage src={assetUrl(comment.userAvatarUrl)} alt={comment.userDisplayName} />
        ) : null}
        <AvatarFallback>{initials}</AvatarFallback>
      </Avatar>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 text-sm">
          <button
            type="button"
            className="font-medium hover:underline"
            onClick={() => navigate(`/users/${comment.userId}`)}
          >
            {comment.userDisplayName}
          </button>
          <span className="text-xs text-muted-foreground">
            {new Date(comment.createdAt).toLocaleString()}
          </span>
        </div>
        <p className="text-sm whitespace-pre-wrap mt-1 break-words">{comment.content}</p>
      </div>
      {canDelete && (
        <Button
          variant="ghost"
          size="sm"
          className="opacity-0 group-hover:opacity-100 transition-opacity"
          onClick={() => {
            if (confirm('Delete this comment?')) onDelete()
          }}
          disabled={isDeleting}
        >
          <Trash2 className="size-4 text-destructive" />
        </Button>
      )}
    </div>
  )
}
