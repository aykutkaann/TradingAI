export const SubscriptionTier = {
  Free: 0,
  Pro: 1,
  Premium: 2,
} as const
export type SubscriptionTier = (typeof SubscriptionTier)[keyof typeof SubscriptionTier]

export const tierLabel = (t: SubscriptionTier) =>
  ({ 0: 'Free', 1: 'Pro', 2: 'Premium' }[t])

export type SubscriptionDto = {
  tier: SubscriptionTier
  planName: string
  dailyAnalysisLimit: number
  dailyPublishLimit: number
  canUseAssetAnalysis: boolean
  canAccessLeaderBoard: boolean
  maxImageSizeMb: number
  expiresAt: string | null
  isActive: boolean
  startedAt: string
}
