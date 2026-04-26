// Mirrors backend enums (serialized as numbers since backend doesn't use JsonStringEnumConverter).
export const AnalysisOutcome = {
  Pending: 0,
  Win: 1,
  Loss: 2,
  Expired: 3,
  Invalidated: 4,
} as const
export type AnalysisOutcome = (typeof AnalysisOutcome)[keyof typeof AnalysisOutcome]

export const AssetType = {
  Crypto: 0,
  Forex: 1,
  Commodity: 2,
  Stock: 3,
} as const
export type AssetType = (typeof AssetType)[keyof typeof AssetType]

export const assetTypeLabel = (t: AssetType) =>
  ({ 0: 'Crypto', 1: 'Forex', 2: 'Commodity', 3: 'Stock' }[t])

export const outcomeLabel = (o: AnalysisOutcome) =>
  ({ 0: 'Pending', 1: 'Win', 2: 'Loss', 3: 'Expired', 4: 'Invalidated' }[o])

export type AssetDto = {
  id: string
  symbol: string
  name: string
  pair: string
  type: AssetType
}

export type PriceDto = {
  assetId: string
  pair: string
  currentPrice: number
  change24h: number
  changePercent24h: number
  high24h: number
  low24h: number
  volume24h: number
  updatedAt: string
}

export type OhlcCandle = {
  timestamp: string
  open: number
  high: number
  low: number
  close: number
  volume: number
}

export type WatchlistItemDto = {
  id: string
  assetId: string
  symbol: string
  name: string
  pair: string
  type: AssetType
  sortOrder: number
  price: PriceDto | null
}

export type AnalysisDto = {
  id: string
  userId: string
  userDisplayName: string
  assetId: string | null
  assetSymbol: string | null
  assetPair: string | null
  timeFrame: string
  chartImageUrl: string | null
  trendDirection: string
  detectedPaterns: string[] // sic — backend has a typo we mirror
  supportLevels: number[]
  resistanceLevels: number[]
  suggestedEntry: number | null
  stopLoss: number | null
  takeProfit1: number | null
  takeProfit2: number | null
  riskRewardRatio: number | null
  analysis: string
  summary: string
  isPublished: boolean
  likeCount: number
  commentCount: number
  outcome: AnalysisOutcome
  tp1Hit: boolean
  tp2Hit: boolean
  slHit: boolean
  resolvedPrice: number | null
  resolvedAt: string | null
  expiresAt: string | null
  createdAt: string
  isLikedByMe: boolean
}

export type PagedResult<T> = {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNext: boolean
  hasPrev: boolean
}

export type AnalyzeAssetRequest = {
  assetId: string
  timeFrame: string
  userPrompt?: string | null
}

export type AnalyzeImageInput = {
  file: File
  assetPair: string
  timeFrame: string
  userPrompt?: string
}
