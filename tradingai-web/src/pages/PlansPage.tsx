import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Check, Star, Zap } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { subscriptionsApi } from '@/api/subscriptions'
import { SubscriptionTier, tierLabel } from '@/types/subscription'
import { getApiErrorMessage } from '@/lib/errors'

type BillingPeriod = 'monthly' | 'annual'

type PlanCard = {
  tier: SubscriptionTier
  name: string
  tagline: string
  priceMonthly: number
  priceAnnual: number // price per month when billed annually
  weeklyOption?: number // for Pro: $3/week alternative
  features: string[]
  hiddenFeatures?: string[]
  popular?: boolean
  ctaGradient: string // tailwind classes
  borderGlow: string
}

const PLANS: PlanCard[] = [
  {
    tier: SubscriptionTier.Free,
    name: 'Free',
    tagline: 'Try Trendox AI on us',
    priceMonthly: 0,
    priceAnnual: 0,
    features: [
      '3 AI chart analyses (lifetime)',
      'Image upload (PNG / JPG / WEBP)',
      'Public feed access',
      'Basic profile',
    ],
    hiddenFeatures: [
      'No tracked-asset analysis',
      'No leaderboard',
    ],
    ctaGradient: 'from-slate-600 to-slate-700',
    borderGlow: 'border-border/50',
  },
  {
    tier: SubscriptionTier.Pro,
    name: 'Pro',
    tagline: 'For active traders',
    priceMonthly: 9.99,
    priceAnnual: 8.25, // ~ $99/yr
    weeklyOption: 3,
    features: [
      '30 AI chart analyses per day',
      'Tracked-asset analysis (live data)',
      'Image upload up to 10 MB',
      'Outcome tracking + email alerts',
      'Leaderboard access',
      'Up to 20 published analyses per day',
    ],
    popular: true,
    ctaGradient: 'from-amber-500 to-purple-600',
    borderGlow: 'border-[#a855f7] shadow-[0_0_40px_rgba(168,85,247,0.25)]',
  },
  {
    tier: SubscriptionTier.Premium,
    name: 'Premium',
    tagline: 'For pros & teams',
    priceMonthly: 29.99,
    priceAnnual: 24.92, // ~ $299/yr
    features: [
      'Unlimited AI chart analyses',
      'Tracked-asset analysis (live data)',
      'Image upload up to 25 MB',
      'Priority outcome tracking',
      'Leaderboard access',
      'Unlimited publishes per day',
      'Early access to new features',
    ],
    ctaGradient: 'from-[#7c3aed] to-[#3b82f6]',
    borderGlow: 'border-border/50',
  },
]

export function PlansPage() {
  const [period, setPeriod] = useState<BillingPeriod>('monthly')
  const queryClient = useQueryClient()

  const { data: current, isLoading } = useQuery({
    queryKey: ['my-subscription'],
    queryFn: subscriptionsApi.getMine,
  })

  const upgradeMutation = useMutation({
    mutationFn: subscriptionsApi.upgrade,
    onSuccess: (data) => {
      queryClient.setQueryData(['my-subscription'], data)
      toast.success(`Upgraded to ${tierLabel(data.tier)}`)
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Upgrade failed')),
  })

  const cancelMutation = useMutation({
    mutationFn: subscriptionsApi.cancel,
    onSuccess: (data) => {
      queryClient.setQueryData(['my-subscription'], data)
      toast.success('Subscription cancelled. You are back on Free.')
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Cancellation failed')),
  })

  return (
    <div className="space-y-10 py-6">
      {/* Header */}
      <div className="text-center max-w-2xl mx-auto space-y-3">
        <h1 className="text-4xl md:text-5xl font-extrabold tracking-tight">
          <span className="bg-gradient-to-r from-[#a855f7] to-[#7c3aed] bg-clip-text text-transparent">
            Starter & Pro Plans
          </span>
        </h1>
        <p className="text-muted-foreground text-base md:text-lg">
          Unlock your trading potential with AI-powered tools and insights
        </p>

        <BillingToggle value={period} onChange={setPeriod} />
      </div>

      {isLoading && <Skeleton className="h-96 w-full max-w-5xl mx-auto" />}

      {current && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5 max-w-5xl mx-auto">
          {PLANS.map((plan) => (
            <PlanCardView
              key={plan.tier}
              plan={plan}
              period={period}
              currentTier={current.tier}
              isPending={upgradeMutation.isPending || cancelMutation.isPending}
              onUpgrade={() => upgradeMutation.mutate(plan.tier)}
              onCancel={() => {
                if (
                  confirm(
                    'Cancel subscription? You will be downgraded to Free at the end of the current period.'
                  )
                ) {
                  cancelMutation.mutate()
                }
              }}
            />
          ))}
        </div>
      )}

      {current?.expiresAt && (
        <p className="text-center text-sm text-muted-foreground">
          Current plan expires {new Date(current.expiresAt).toLocaleDateString()}
        </p>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
function BillingToggle({
  value,
  onChange,
}: {
  value: BillingPeriod
  onChange: (v: BillingPeriod) => void
}) {
  return (
    <div className="inline-flex items-center gap-3 pt-2">
      <button
        type="button"
        onClick={() => onChange('monthly')}
        className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors ${
          value === 'monthly'
            ? 'bg-[#a855f7] text-white'
            : 'text-muted-foreground hover:text-foreground'
        }`}
      >
        Monthly
      </button>
      <button
        type="button"
        onClick={() => onChange(value === 'annual' ? 'monthly' : 'annual')}
        className="relative w-12 h-6 rounded-full bg-muted-foreground/20"
        aria-label="Toggle annual billing"
      >
        <span
          className={`absolute top-0.5 size-5 rounded-full bg-white transition-all ${
            value === 'annual' ? 'left-6 bg-[#a855f7]' : 'left-0.5'
          }`}
        />
      </button>
      <button
        type="button"
        onClick={() => onChange('annual')}
        className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors ${
          value === 'annual'
            ? 'bg-[#a855f7] text-white'
            : 'text-muted-foreground hover:text-foreground'
        }`}
      >
        Annual{' '}
        <span className="text-[10px] text-emerald-400 ml-1">save ~17%</span>
      </button>
    </div>
  )
}

// ---------------------------------------------------------------------------
function PlanCardView({
  plan,
  period,
  currentTier,
  isPending,
  onUpgrade,
  onCancel,
}: {
  plan: PlanCard
  period: BillingPeriod
  currentTier: SubscriptionTier
  isPending: boolean
  onUpgrade: () => void
  onCancel: () => void
}) {
  const isCurrent = plan.tier === currentTier
  const isUpgrade = plan.tier > currentTier
  const isDowngrade = plan.tier < currentTier
  const isFree = plan.tier === SubscriptionTier.Free

  const displayPrice =
    period === 'annual' && plan.priceAnnual > 0 ? plan.priceAnnual : plan.priceMonthly

  return (
    <div
      className={`relative rounded-2xl border-2 bg-card/40 backdrop-blur p-6 flex flex-col transition-all ${plan.borderGlow}`}
    >
      {plan.popular && (
        <div className="absolute -top-3 left-1/2 -translate-x-1/2 bg-gradient-to-r from-amber-400 to-orange-500 text-black px-3 py-1 rounded-full text-xs font-bold flex items-center gap-1 shadow-lg">
          <Star className="size-3 fill-current" />
          MOST POPULAR
        </div>
      )}

      {/* Title */}
      <div className="text-center space-y-1 mb-5">
        <h3 className={`text-2xl font-bold ${plan.popular ? 'text-[#a855f7]' : ''}`}>
          {plan.name}
        </h3>
        <p className="text-sm text-muted-foreground">{plan.tagline}</p>
      </div>

      {/* Price */}
      <div className="text-center mb-5">
        {isFree ? (
          <div className="text-4xl font-extrabold">Free</div>
        ) : (
          <>
            <div className="flex items-baseline justify-center gap-1">
              <span
                className={`text-5xl font-extrabold bg-gradient-to-r bg-clip-text text-transparent ${
                  plan.popular
                    ? 'from-amber-400 to-purple-500'
                    : 'from-[#a855f7] to-[#7c3aed]'
                }`}
              >
                ${displayPrice.toFixed(2)}
              </span>
              <span className="text-muted-foreground">/month</span>
            </div>
            {plan.weeklyOption && period === 'monthly' && (
              <p className="text-xs text-muted-foreground mt-1">
                or <strong>${plan.weeklyOption}/week</strong>
              </p>
            )}
            {period === 'annual' && (
              <p className="text-xs text-muted-foreground mt-1">
                billed annually
              </p>
            )}
          </>
        )}
      </div>

      {/* Features */}
      <ul className="space-y-2.5 mb-6 flex-1">
        {plan.features.map((f) => (
          <li key={f} className="flex items-start gap-2 text-sm">
            <span className="size-5 rounded-full bg-emerald-500 flex items-center justify-center shrink-0 mt-0.5">
              <Check className="size-3 text-white" />
            </span>
            <span>{f}</span>
          </li>
        ))}
        {plan.hiddenFeatures?.map((f) => (
          <li
            key={f}
            className="flex items-start gap-2 text-sm text-muted-foreground"
          >
            <span className="size-5 rounded-full border border-muted-foreground/30 flex items-center justify-center shrink-0 mt-0.5" />
            <span>{f}</span>
          </li>
        ))}
      </ul>

      {/* CTA */}
      <PlanCta
        plan={plan}
        isCurrent={isCurrent}
        isUpgrade={isUpgrade}
        isDowngrade={isDowngrade}
        isPending={isPending}
        onUpgrade={onUpgrade}
        onCancel={onCancel}
      />
    </div>
  )
}

// ---------------------------------------------------------------------------
function PlanCta({
  plan,
  isCurrent,
  isUpgrade,
  isDowngrade,
  isPending,
  onUpgrade,
  onCancel,
}: {
  plan: PlanCard
  isCurrent: boolean
  isUpgrade: boolean
  isDowngrade: boolean
  isPending: boolean
  onUpgrade: () => void
  onCancel: () => void
}) {
  if (isCurrent) {
    if (plan.tier === SubscriptionTier.Free) {
      return (
        <Button variant="outline" className="w-full h-11" disabled>
          Current plan
        </Button>
      )
    }
    return (
      <Button
        variant="outline"
        className="w-full h-11"
        onClick={onCancel}
        disabled={isPending}
      >
        Cancel
      </Button>
    )
  }

  if (isUpgrade) {
    const label = plan.tier === SubscriptionTier.Premium ? 'Get Premium' : `Get ${plan.name}`
    return (
      <Button
        className={`w-full h-11 bg-gradient-to-r ${plan.ctaGradient} text-white hover:opacity-90 font-semibold`}
        onClick={onUpgrade}
        disabled={isPending}
      >
        {plan.popular && <Zap className="size-4 mr-1 fill-current" />}
        {label}
      </Button>
    )
  }

  if (isDowngrade) {
    return (
      <Button variant="outline" className="w-full h-11" disabled>
        Cancel current plan first
      </Button>
    )
  }

  return null
}
