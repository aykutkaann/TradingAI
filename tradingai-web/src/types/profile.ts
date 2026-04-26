export type PublicProfileDto = {
  id: string
  userName: string
  displayName: string | null
  avatarUrl: string | null
  bio: string | null
  followersCount: number
  followingCount: number
  analysesCount: number
  isFollowedByMe: boolean
  createdAt: string
}

export type UserStatsDto = {
  totalAnalyses: number
  publishedAnalyses: number
  wins: number
  losses: number
  expired: number
  pending: number
  winRate: number
  tp2HitCount: number
  tp2HitRate: number
}
