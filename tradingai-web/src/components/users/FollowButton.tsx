import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { UserPlus, UserCheck } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { usersApi } from '@/api/users'
import { useAuthStore } from '@/stores/authStore'
import { getApiErrorMessage } from '@/lib/errors'
import type { PublicProfileDto } from '@/types/profile'

type Props = {
  profile: PublicProfileDto
}

/**
 * Follow/unfollow with optimistic update — same pattern as LikeButton.
 * Hidden when viewing your own profile (you can't follow yourself).
 */
export function FollowButton({ profile }: Props) {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const currentUserId = useAuthStore((s) => s.user?.id)

  const queryKey = ['profile', profile.id]

  const mutation = useMutation({
    mutationFn: () =>
      profile.isFollowedByMe ? usersApi.unfollow(profile.id) : usersApi.follow(profile.id),

    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey })
      const previous = queryClient.getQueryData<PublicProfileDto>(queryKey)

      if (previous) {
        queryClient.setQueryData<PublicProfileDto>(queryKey, {
          ...previous,
          isFollowedByMe: !previous.isFollowedByMe,
          followersCount: previous.followersCount + (previous.isFollowedByMe ? -1 : 1),
        })
      }
      return { previous }
    },

    onError: (err, _vars, context) => {
      if (context?.previous) queryClient.setQueryData(queryKey, context.previous)
      toast.error(getApiErrorMessage(err, 'Action failed'))
    },

    onSettled: () => {
      queryClient.invalidateQueries({ queryKey })
      // The following feed depends on this — invalidate so the feed
      // refetches next time the user opens it.
      queryClient.invalidateQueries({ queryKey: ['following-feed'] })
    },
  })

  // Don't show on own profile
  if (currentUserId === profile.id) return null

  const handleClick = () => {
    if (!isAuthenticated) {
      toast.info('Sign in to follow users')
      navigate('/login')
      return
    }
    mutation.mutate()
  }

  return (
    <Button
      variant={profile.isFollowedByMe ? 'outline' : 'default'}
      onClick={handleClick}
      disabled={mutation.isPending}
    >
      {profile.isFollowedByMe ? (
        <>
          <UserCheck className="size-4 mr-1" /> Following
        </>
      ) : (
        <>
          <UserPlus className="size-4 mr-1" /> Follow
        </>
      )}
    </Button>
  )
}
