import { z } from 'zod'

// Must match backend AnalyzeAssetCommandValidator's allowed list (case-sensitive).
export const TIMEFRAMES = ['1m', '5m', '15m', '30m', '1h', '4h', '1d'] as const

export const analyzeAssetSchema = z.object({
  assetId: z.string().min(1, 'Pick an asset'),
  timeFrame: z.string().min(1, 'Pick a timeframe'),
  userPrompt: z.string().max(500, 'Max 500 characters').optional(),
})

export const analyzeImageSchema = z.object({
  assetPair: z
    .string()
    .min(1, 'Asset pair is required (e.g. BTC/USDT)')
    .max(20, 'Too long'),
  timeFrame: z.string().min(1, 'Pick a timeframe'),
  userPrompt: z.string().max(500, 'Max 500 characters').optional(),
})

export type AnalyzeAssetFormValues = z.infer<typeof analyzeAssetSchema>
export type AnalyzeImageFormValues = z.infer<typeof analyzeImageSchema>
