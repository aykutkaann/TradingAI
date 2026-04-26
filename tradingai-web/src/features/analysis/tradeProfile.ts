import { useEffect, useState } from 'react'

// Local-only for now. When the backend gets a /trade-profile endpoint,
// this hook is the single place to swap localStorage for an API call —
// nothing in the UI cares where the value comes from.

export type TradeProfile = {
  riskBudgetMode: 'percent' | 'fixed'
  accountBalance: number
  riskPercent: number
  riskFixed: number
  style: 'conservative' | 'balanced' | 'aggressive'
  useInAnalysis: boolean
  stopPreference: 'tight' | 'balanced' | 'wide'
  targetCount: 1 | 2 | 3
  minRR: string // e.g. "1.8:1"
}

const DEFAULT_PROFILE: TradeProfile = {
  riskBudgetMode: 'percent',
  accountBalance: 10000,
  riskPercent: 1,
  riskFixed: 100,
  style: 'balanced',
  useInAnalysis: false,
  stopPreference: 'balanced',
  targetCount: 2,
  minRR: '1.8:1',
}

const STORAGE_KEY = 'trade-profile'

function read(): TradeProfile {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return DEFAULT_PROFILE
    const parsed = JSON.parse(raw)
    return { ...DEFAULT_PROFILE, ...parsed }
  } catch {
    return DEFAULT_PROFILE
  }
}

/**
 * Hook: returns the current profile + a setter that auto-persists.
 * No need to manually call `save()` — every update is saved.
 */
export function useTradeProfile() {
  const [profile, setProfileState] = useState<TradeProfile>(() => read())

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(profile))
  }, [profile])

  const update = (patch: Partial<TradeProfile>) =>
    setProfileState((p) => ({ ...p, ...patch }))

  const reset = () => setProfileState(DEFAULT_PROFILE)

  return { profile, update, reset }
}

/**
 * Build a free-text instruction block for the AI from the trade profile.
 * Appended to the user's own prompt when "Use Trade Profile in analysis" is on.
 */
export function buildProfilePrompt(p: TradeProfile): string {
  const riskLine =
    p.riskBudgetMode === 'percent'
      ? `Account balance: $${p.accountBalance}. Risking ${p.riskPercent}% per trade (= $${(
          (p.accountBalance * p.riskPercent) / 100
        ).toFixed(2)}).`
      : `Risking a fixed $${p.riskFixed} per trade.`

  return [
    'Please tailor the trade levels to my profile:',
    riskLine,
    `Style: ${p.style}.`,
    `Stop preference: ${p.stopPreference} (buffer around invalidation, not the stop itself).`,
    `Targets: ${p.targetCount}.`,
    `Minimum acceptable risk:reward = ${p.minRR}.`,
  ].join('\n')
}
