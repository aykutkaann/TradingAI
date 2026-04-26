import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { assetUrl } from '@/lib/assetUrl'

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Separator } from '@/components/ui/separator'
import { useAuthStore } from '@/stores/authStore'
import { subscriptionsApi } from '@/api/subscriptions'
import { tierLabel, type SubscriptionDto } from '@/types/subscription'
import { ArrowUpRight, Sparkles } from 'lucide-react'

export function ProfilePage() {
  const user = useAuthStore((s) => s.user)

  const subscriptionQuery = useQuery({
    queryKey: ['my-subscription'],
    queryFn: subscriptionsApi.getMine,
  })

  if (!user) return null

  const initials = (user.displayName ?? user.userName).slice(0, 2).toUpperCase()

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      {/* Identity card */}
      <Card>
        <CardHeader className="flex flex-row items-center gap-4">
          <Avatar className="size-16">
            {user.avatarUrl ? <AvatarImage src={assetUrl(user.avatarUrl)} alt={user.userName} /> : null}
            <AvatarFallback className="text-lg">{initials}</AvatarFallback>
          </Avatar>
          <div>
            <CardTitle className="text-2xl">{user.displayName ?? user.userName}</CardTitle>
            <CardDescription>@{user.userName}</CardDescription>
          </div>
        </CardHeader>
        <CardContent className="text-sm">
          <div className="flex justify-between py-2">
            <span className="text-muted-foreground">Email</span>
            <span>{user.email}</span>
          </div>
        </CardContent>
      </Card>

      {/* Subscription card */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between space-y-0">
          <div>
            <CardTitle>Subscription</CardTitle>
            <CardDescription>Your current plan and what's included</CardDescription>
          </div>
          {subscriptionQuery.data && (
            <Badge variant={subscriptionQuery.data.tier === 0 ? 'secondary' : 'default'}>
              {tierLabel(subscriptionQuery.data.tier)}
            </Badge>
          )}
        </CardHeader>
        <CardContent className="space-y-4">
          {subscriptionQuery.isLoading && <Skeleton className="h-32 w-full" />}

          {subscriptionQuery.data && (
            <SubscriptionDetails sub={subscriptionQuery.data} />
          )}

          <Separator />

          <div className="flex items-center justify-between flex-wrap gap-2">
            <p className="text-sm text-muted-foreground">
              Want more analyses or asset access? See all plans.
            </p>
            <Button asChild size="sm">
              <Link to="/plans">
                <Sparkles className="size-4 mr-1" /> View plans
                <ArrowUpRight className="size-3 ml-1" />
              </Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

// ---------------------------------------------------------------------------
// Subscription detail rows. Daily limits come from the backend SubscriptionDto.
// "Used today" is not in the DTO yet — when the backend exposes a usage
// endpoint we'll fill the second number in. For now we just show the limit.
// ---------------------------------------------------------------------------
function SubscriptionDetails({ sub }: { sub: SubscriptionDto }) {
  const fmtLimit = (n: number) => (n >= 9999 ? 'Unlimited' : n.toString())

  const rows: { label: string; value: React.ReactNode }[] = [
    { label: 'Plan', value: sub.planName },
    { label: 'Status', value: sub.isActive ? 'Active' : 'Inactive' },
    {
      label: 'Started',
      value: new Date(sub.startedAt).toLocaleDateString(),
    },
    {
      label: 'Renews / expires',
      value: sub.expiresAt ? new Date(sub.expiresAt).toLocaleDateString() : '—',
    },
  ]

  const usage: { label: string; value: React.ReactNode; included: boolean }[] = [
    {
      label: 'Daily analyses',
      value: fmtLimit(sub.dailyAnalysisLimit),
      included: true,
    },
    {
      label: 'Daily publishes',
      value: fmtLimit(sub.dailyPublishLimit),
      included: true,
    },
    {
      label: 'Image upload size',
      value: `${sub.maxImageSizeMb} MB`,
      included: true,
    },
    {
      label: 'Tracked-asset analysis',
      value: sub.canUseAssetAnalysis ? 'Included' : 'Not included',
      included: sub.canUseAssetAnalysis,
    },
    {
      label: 'Leaderboard access',
      value: sub.canAccessLeaderBoard ? 'Included' : 'Not included',
      included: sub.canAccessLeaderBoard,
    },
  ]

  return (
    <div className="space-y-4 text-sm">
      <div>
        <h3 className="font-semibold text-xs text-muted-foreground uppercase tracking-wide mb-2">
          Plan
        </h3>
        <div className="space-y-1">
          {rows.map((r) => (
            <div key={r.label} className="flex justify-between py-1.5 border-b border-border last:border-0">
              <span className="text-muted-foreground">{r.label}</span>
              <span>{r.value}</span>
            </div>
          ))}
        </div>
      </div>

      <div>
        <h3 className="font-semibold text-xs text-muted-foreground uppercase tracking-wide mb-2">
          Usage limits
        </h3>
        <div className="space-y-1">
          {usage.map((u) => (
            <div key={u.label} className="flex justify-between py-1.5 border-b border-border last:border-0">
              <span className="text-muted-foreground">{u.label}</span>
              <span className={u.included ? '' : 'text-muted-foreground'}>{u.value}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
