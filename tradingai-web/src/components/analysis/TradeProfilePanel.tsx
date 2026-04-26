import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Save, ChevronDown, ChevronUp, Sliders, Sparkles } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { useTradeProfile } from '@/features/analysis/tradeProfile'
import { analysisApi } from '@/api/analysis'
import { AnalysisListCard } from '@/components/analysis/AnalysisListCard'

const RR_OPTIONS = ['1:1', '1.5:1', '1.8:1', '2:1', '2.5:1', '3:1']

/**
 * Right-hand panel on the Analyze page. Two tabs: "Trade Profile" (settings
 * the user wants applied to every analysis) and "History" (their past runs).
 *
 * Settings auto-persist via the useTradeProfile hook → localStorage.
 * Manual "Save Profile" button just gives the user explicit confirmation.
 */
export function TradeProfilePanel() {
  return (
    <Tabs defaultValue="profile" className="w-full">
      <TabsList className="grid w-full grid-cols-2">
        <TabsTrigger value="profile">Trade Profile</TabsTrigger>
        <TabsTrigger value="history">History</TabsTrigger>
      </TabsList>

      <TabsContent value="profile" className="mt-4">
        <ProfileForm />
      </TabsContent>

      <TabsContent value="history" className="mt-4">
        <HistoryList />
      </TabsContent>
    </Tabs>
  )
}

// ---------------------------------------------------------------------------
function ProfileForm() {
  const { profile, update } = useTradeProfile()
  const [moreOpen, setMoreOpen] = useState(false)

  const riskAmount =
    profile.riskBudgetMode === 'percent'
      ? (profile.accountBalance * profile.riskPercent) / 100
      : profile.riskFixed

  return (
    <div className="rounded-xl border border-border bg-card/40 backdrop-blur p-4 space-y-5">
      <div className="space-y-1">
        <h3 className="font-semibold flex items-center gap-2">
          <Sliders className="size-4 text-[#a855f7]" />
          Trade Profile
        </h3>
        <p className="text-xs text-muted-foreground">
          Set your risk rules. Trendox AI will tailor SL/TP to match.
        </p>
      </div>

      {/* Risk Budget */}
      <div className="space-y-2">
        <Label className="text-xs font-medium">Risk Budget</Label>
        <div className="grid grid-cols-2 gap-2">
          <SegButton
            active={profile.riskBudgetMode === 'percent'}
            onClick={() => update({ riskBudgetMode: 'percent' })}
          >
            % of Account
          </SegButton>
          <SegButton
            active={profile.riskBudgetMode === 'fixed'}
            onClick={() => update({ riskBudgetMode: 'fixed' })}
          >
            Fixed $ Risk
          </SegButton>
        </div>
      </div>

      {/* Account / risk inputs depend on mode */}
      {profile.riskBudgetMode === 'percent' ? (
        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1">
            <Label className="text-xs">Account Balance ($)</Label>
            <Input
              type="number"
              min={0}
              value={profile.accountBalance}
              onChange={(e) =>
                update({ accountBalance: Number(e.target.value) || 0 })
              }
            />
          </div>
          <div className="space-y-1">
            <Label className="text-xs">Risk % per trade</Label>
            <Input
              type="number"
              min={0}
              step="0.1"
              value={profile.riskPercent}
              onChange={(e) =>
                update({ riskPercent: Number(e.target.value) || 0 })
              }
            />
          </div>
        </div>
      ) : (
        <div className="space-y-1">
          <Label className="text-xs">Fixed risk per trade ($)</Label>
          <Input
            type="number"
            min={0}
            value={profile.riskFixed}
            onChange={(e) => update({ riskFixed: Number(e.target.value) || 0 })}
          />
        </div>
      )}

      <p className="text-[11px] text-muted-foreground">
        Recommended: 0.5%–1% per trade depending on trade type
      </p>

      {/* Style */}
      <div className="space-y-2">
        <Label className="text-xs font-medium">Style</Label>
        <div className="grid grid-cols-3 gap-2">
          {(['conservative', 'balanced', 'aggressive'] as const).map((s) => (
            <SegButton
              key={s}
              active={profile.style === s}
              onClick={() => update({ style: s })}
            >
              <span className="capitalize">{s}</span>
            </SegButton>
          ))}
        </div>
      </div>

      {/* Toggle */}
      <div className="rounded-lg border border-border p-3 flex items-center justify-between gap-3">
        <div className="min-w-0">
          <p className="text-sm font-medium">Use Trade Profile in analysis</p>
          <p className="text-xs text-muted-foreground">
            AI will tailor SL/TP to your rules
          </p>
        </div>
        <Switch
          checked={profile.useInAnalysis}
          onCheckedChange={(v) => update({ useInAnalysis: v })}
        />
      </div>

      {/* More controls */}
      <button
        type="button"
        className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
        onClick={() => setMoreOpen((v) => !v)}
      >
        {moreOpen ? <ChevronUp className="size-4" /> : <ChevronDown className="size-4" />}
        More controls
      </button>

      {moreOpen && (
        <div className="space-y-4 border-t border-border pt-4">
          <div className="space-y-2">
            <Label className="text-xs font-medium">Stop Preference</Label>
            <div className="grid grid-cols-3 gap-2">
              {(['tight', 'balanced', 'wide'] as const).map((s) => (
                <SegButton
                  key={s}
                  active={profile.stopPreference === s}
                  onClick={() => update({ stopPreference: s })}
                >
                  <span className="capitalize">{s}</span>
                </SegButton>
              ))}
            </div>
            <p className="text-[11px] text-muted-foreground">
              Buffer around invalidation — not the stop itself
            </p>
          </div>

          <div className="space-y-2">
            <Label className="text-xs font-medium">Target Preference</Label>
            <div className="grid grid-cols-3 gap-2">
              {([1, 2, 3] as const).map((n) => (
                <SegButton
                  key={n}
                  active={profile.targetCount === n}
                  onClick={() => update({ targetCount: n })}
                >
                  {n} {n === 1 ? 'Target' : 'Targets'}
                </SegButton>
              ))}
            </div>
          </div>

          <div className="space-y-1">
            <Label className="text-xs">Minimum R:R</Label>
            <Select value={profile.minRR} onValueChange={(v) => update({ minRR: v })}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {RR_OPTIONS.map((rr) => (
                  <SelectItem key={rr} value={rr}>
                    {rr}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      )}

      {/* Summary card */}
      <div className="rounded-lg border border-border bg-card p-3 text-xs space-y-1">
        <p className="font-semibold tracking-wider uppercase text-muted-foreground text-[10px]">
          Summary
        </p>
        <p>
          <span className="text-muted-foreground">Risk:</span>{' '}
          <strong>${riskAmount.toFixed(2)}</strong>{' '}
          {profile.riskBudgetMode === 'percent' && (
            <span className="text-muted-foreground">
              ({profile.riskPercent}% of ${profile.accountBalance})
            </span>
          )}
        </p>
        <p>
          <span className="text-muted-foreground">Style:</span>{' '}
          <strong className="capitalize">{profile.style}</strong>
        </p>
        <p>
          <span className="text-muted-foreground">Min RR:</span>{' '}
          <strong>{profile.minRR}</strong>
          {' · '}
          <span className="text-muted-foreground">Targets:</span>{' '}
          <strong>{profile.targetCount}</strong>
          {' · '}
          <span className="text-muted-foreground">Stop:</span>{' '}
          <strong className="capitalize">{profile.stopPreference}</strong>
        </p>
      </div>

      <Button
        className="w-full bg-[#a855f7] hover:bg-[#9333ea]"
        onClick={() => toast.success('Profile saved')}
      >
        <Save className="size-4 mr-2" />
        Save Profile
      </Button>
    </div>
  )
}

// ---------------------------------------------------------------------------
function HistoryList() {
  const { data, isLoading } = useQuery({
    queryKey: ['my-analyses', 'history-side', 1],
    queryFn: () => analysisApi.getMine(1, 6),
  })

  return (
    <div className="rounded-xl border border-border bg-card/40 backdrop-blur p-4 space-y-3">
      <h3 className="font-semibold flex items-center gap-2">
        <Sparkles className="size-4 text-[#a855f7]" />
        Recent runs
      </h3>

      {isLoading && (
        <div className="space-y-2">
          <Skeleton className="h-20 w-full" />
          <Skeleton className="h-20 w-full" />
        </div>
      )}

      {data && data.items.length === 0 && (
        <p className="text-sm text-muted-foreground py-4 text-center">
          No analyses yet. Run your first one →
        </p>
      )}

      {data && data.items.length > 0 && (
        <div className="space-y-2">
          {data.items.map((a) => (
            <AnalysisListCard key={a.id} analysis={a} />
          ))}
          {data.totalCount > 6 && (
            <Button asChild variant="ghost" size="sm" className="w-full">
              <Link to="/analyses">View all</Link>
            </Button>
          )}
        </div>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Segmented button used for the "tabs" inside the form (% / Fixed, etc.)
function SegButton({
  active,
  onClick,
  children,
}: {
  active: boolean
  onClick: () => void
  children: React.ReactNode
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`h-9 text-sm rounded-md transition-colors ${
        active
          ? 'bg-[#a855f7] text-white font-medium'
          : 'bg-card border border-border text-muted-foreground hover:text-foreground'
      }`}
    >
      {children}
    </button>
  )
}
