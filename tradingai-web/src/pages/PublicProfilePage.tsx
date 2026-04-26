import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { assetUrl } from '@/lib/assetUrl'

import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { usersApi } from '@/api/users'
import { FollowButton } from '@/components/users/FollowButton'
import type { UserStatsDto } from '@/types/profile'

export function PublicProfilePage() {
  const { id } = useParams<{ id: string }>()

  const profileQuery = useQuery({
    queryKey: ['profile', id],
    queryFn: () => usersApi.getProfile(id!),
    enabled: !!id,
  })

  const statsQuery = useQuery({
    queryKey: ['user-stats', id],
    queryFn: () => usersApi.getStats(id!),
    enabled: !!id,
  })

  if (profileQuery.isLoading) {
    return <Skeleton className="h-[400px] w-full max-w-3xl mx-auto" />
  }

  if (profileQuery.isError || !profileQuery.data) {
    return (
      <div className="max-w-2xl mx-auto text-center py-16">
        <p className="text-destructive mb-4">User not found.</p>
        <Button asChild variant="outline">
          <Link to="/feed">Back to feed</Link>
        </Button>
      </div>
    )
  }

  const profile = profileQuery.data
  const stats = statsQuery.data
  const initials = (profile.displayName ?? profile.userName).slice(0, 2).toUpperCase()

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <Card>
        <CardContent className="p-6 space-y-4">
          <div className="flex flex-col sm:flex-row sm:items-start gap-4">
            <Avatar className="size-20">
              {profile.avatarUrl ? (
                <AvatarImage src={assetUrl(profile.avatarUrl)} alt={profile.userName} />
              ) : null}
              <AvatarFallback className="text-xl">{initials}</AvatarFallback>
            </Avatar>

            <div className="flex-1 min-w-0">
              <div className="flex items-center justify-between gap-2 flex-wrap">
                <div>
                  <h1 className="text-2xl font-bold">
                    {profile.displayName ?? profile.userName}
                  </h1>
                  <p className="text-muted-foreground">@{profile.userName}</p>
                </div>
                <FollowButton profile={profile} />
              </div>

              {profile.bio && (
                <p className="text-sm mt-3 whitespace-pre-wrap">{profile.bio}</p>
              )}

              <p className="text-xs text-muted-foreground mt-2">
                Joined {new Date(profile.createdAt).toLocaleDateString()}
              </p>
            </div>
          </div>

          <Separator />

          <div className="flex gap-6 text-sm">
            <div>
              <span className="font-bold">{profile.followersCount}</span>{' '}
              <span className="text-muted-foreground">Followers</span>
            </div>
            <div>
              <span className="font-bold">{profile.followingCount}</span>{' '}
              <span className="text-muted-foreground">Following</span>
            </div>
            <div>
              <span className="font-bold">{profile.analysesCount}</span>{' '}
              <span className="text-muted-foreground">Analyses</span>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Stats */}
      <Card>
        <CardContent className="p-6">
          <h2 className="text-xl font-semibold mb-4">Performance</h2>
          {statsQuery.isLoading && <Skeleton className="h-32 w-full" />}
          {stats && <StatsGrid stats={stats} />}
        </CardContent>
      </Card>
    </div>
  )
}

function StatsGrid({ stats }: { stats: UserStatsDto }) {
  const items = [
    { label: 'Win Rate', value: `${(stats.winRate * 100).toFixed(1)}%` },
    { label: 'TP2 Hit Rate', value: `${(stats.tp2HitRate * 100).toFixed(1)}%` },
    { label: 'Wins', value: stats.wins },
    { label: 'Losses', value: stats.losses },
    { label: 'Pending', value: stats.pending },
    { label: 'Expired', value: stats.expired },
    { label: 'Published', value: stats.publishedAnalyses },
    { label: 'Total', value: stats.totalAnalyses },
  ]
  return (
    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
      {items.map((s) => (
        <div key={s.label} className="border border-border rounded-md p-3">
          <p className="text-xs text-muted-foreground">{s.label}</p>
          <p className="text-2xl font-bold">{s.value}</p>
        </div>
      ))}
    </div>
  )
}
