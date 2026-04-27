import { Link, useNavigate } from 'react-router-dom'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { assetUrl } from '@/lib/assetUrl'
import {
  TrendingUp,
  TrendingDown,
  Minus,
  Copy,
  Plus,
  ArrowRight,
} from 'lucide-react'
import { type AnalysisDto, outcomeLabel } from '@/types/analysis'
import { toast } from 'sonner'

type Props = {
  analysis: AnalysisDto
}

const fmt = (n: number | null | undefined, digits = 2) =>
  n === null || n === undefined ? '—' : Number(n).toFixed(digits)

// Map trend direction to a side label + colour, screenshot-style.
function sideForTrend(trend: string): { label: string; color: string; icon: typeof TrendingUp } {
  const t = trend.toLowerCase()
  if (t === 'bullish')
    return { label: 'BUY', color: 'bg-emerald-500 text-white', icon: TrendingUp }
  if (t === 'bearish')
    return { label: 'SELL', color: 'bg-red-500 text-white', icon: TrendingDown }
  return { label: 'NEUTRAL', color: 'bg-muted text-muted-foreground', icon: Minus }
}

export function AnalysisResultCard({ analysis: a }: Props) {
  const navigate = useNavigate()
  const side = sideForTrend(a.trendDirection)
  const SideIcon = side.icon
  const ticker = a.assetPair ?? a.assetSymbol ?? 'Image'

  const handleCopyPlan = () => {
    const lines = [
      `${side.label} ${ticker} (${a.timeFrame})`,
      `Entry: ${fmt(a.suggestedEntry)}`,
      `Stop Loss: ${fmt(a.stopLoss)}`,
      `Take Profit 1: ${fmt(a.takeProfit1)}`,
      `Take Profit 2: ${fmt(a.takeProfit2)}`,
      `R:R: ${fmt(a.riskRewardRatio)}`,
      ``,
      a.summary,
    ]
    navigator.clipboard.writeText(lines.join('\n'))
    toast.success('Trade plan copied to clipboard')
  }

  // Risk % (entry → SL) on a 1-unit position basis. Pure display number.
  const riskPct =
    a.suggestedEntry && a.stopLoss
      ? Math.abs((a.suggestedEntry - a.stopLoss) / a.suggestedEntry) * 100
      : null

  // R:R for each TP relative to risk size.
  const riskSize =
    a.suggestedEntry && a.stopLoss ? Math.abs(a.suggestedEntry - a.stopLoss) : null
  const tp1Rr =
    riskSize && a.suggestedEntry && a.takeProfit1
      ? Math.abs(a.takeProfit1 - a.suggestedEntry) / riskSize
      : null
  const tp2Rr =
    riskSize && a.suggestedEntry && a.takeProfit2
      ? Math.abs(a.takeProfit2 - a.suggestedEntry) / riskSize
      : null

  return (
    <div className="space-y-4">
      {/* ============================================================ */}
      {/* TOP ACTION ROW                                                */}
      {/* ============================================================ */}
      <div className="flex items-center justify-between gap-3 flex-wrap">
        <div className="flex items-center gap-2 flex-wrap">
          <Badge className={`${side.color} font-bold px-3 py-1 text-xs`}>
            <SideIcon className="size-3 mr-1" /> {side.label}
          </Badge>
          <Badge variant="outline" className="font-mono px-3 py-1 text-xs">
            {ticker.replace('/', '')}
          </Badge>
          {a.riskRewardRatio !== null && (
            <Badge variant="outline" className="text-[#a855f7] border-[#a855f7]/40 px-3 py-1 text-xs">
              R:R {fmt(a.riskRewardRatio)}:1
            </Badge>
          )}
          <Badge variant="secondary" className="text-xs px-3 py-1">
            {a.timeFrame}
          </Badge>
        </div>

        <div className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={handleCopyPlan}>
            <Copy className="size-3.5 mr-1" /> Copy Plan
          </Button>
          <Button
            size="sm"
            className="bg-[#a855f7] hover:bg-[#9333ea]"
            onClick={() => navigate('/analyze')}
          >
            <Plus className="size-3.5 mr-1" /> New Analysis
          </Button>
        </div>
      </div>

      {/* Caption: author + timestamp */}
      <p className="text-xs text-muted-foreground">
        by{' '}
        <Link to={`/users/${a.userId}`} className="hover:underline text-foreground">
          {a.userDisplayName}
        </Link>{' '}
        · {new Date(a.createdAt).toLocaleString()} · Outcome:{' '}
        <span className="font-medium">{outcomeLabel(a.outcome)}</span>
        {a.isPublished && <span className="ml-2 text-[#a855f7]">· Published</span>}
      </p>

      {/* ============================================================ */}
      {/* CHART + TRADE PLAN — main two-column card                     */}
      {/* ============================================================ */}
      <Card className="overflow-hidden">
        <CardContent className="p-0">
          <div className="grid grid-cols-1 lg:grid-cols-3">
            {/* LEFT: chart image / placeholder */}
            <div className="lg:col-span-2 bg-black/40 min-h-[320px] flex items-center justify-center">
              {a.chartImageUrl ? (
                <img
                  src={assetUrl(a.chartImageUrl)}
                  alt="Chart"
                  className="w-full h-auto block max-h-[500px] object-contain"
                />
              ) : (
                <div className="text-center p-8 space-y-2 text-muted-foreground">
                  <p className="text-sm">Live chart not available for tracked-asset analyses.</p>
                  <p className="text-xs">The trade plan on the right was computed from live OHLC data.</p>
                </div>
              )}
            </div>

            {/* RIGHT: trade plan sidebar */}
            <div className="lg:col-span-1 border-t lg:border-t-0 lg:border-l border-border p-4 space-y-3 bg-card/40">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold flex items-center gap-2">
                  Trade Plan{' '}
                  <Badge className={`${side.color} text-[10px] px-2 py-0.5`}>
                    {side.label}
                  </Badge>
                </h3>
                {a.riskRewardRatio !== null && (
                  <span className="text-xs text-[#a855f7] font-medium">
                    R:R {fmt(a.riskRewardRatio)}:1
                  </span>
                )}
              </div>

              <div className="flex items-center gap-2 flex-wrap">
                <Badge variant="outline" className="text-[10px]">
                  {a.timeFrame}
                </Badge>
                <Badge variant="outline" className="text-[10px] capitalize">
                  {a.trendDirection}
                </Badge>
              </div>

              {/* Entry */}
              <PlanRow
                label="Entry"
                sub={`${side.label} entry`}
                value={fmt(a.suggestedEntry)}
                badge="ENTRY"
                badgeClass="bg-muted text-foreground"
              />

              {/* Stop Loss */}
              <PlanRow
                label="Stop Loss"
                sub={
                  riskPct !== null ? `Risk: ${riskPct.toFixed(2)}%` : 'Not set'
                }
                value={fmt(a.stopLoss)}
                badge="SL"
                badgeClass="bg-red-500/15 text-red-400 border border-red-500/30"
                tone="danger"
              />

              {/* TP1 */}
              <PlanRow
                label="Target 1"
                sub={tp1Rr !== null ? `RR: ${tp1Rr.toFixed(2)}:1` : 'Not set'}
                value={fmt(a.takeProfit1)}
                badge="TP1"
                badgeClass="bg-emerald-500/15 text-emerald-400 border border-emerald-500/30"
                tone="success"
              />

              {/* TP2 */}
              {a.takeProfit2 !== null && (
                <PlanRow
                  label="Target 2"
                  sub={tp2Rr !== null ? `RR: ${tp2Rr.toFixed(2)}:1` : 'Not set'}
                  value={fmt(a.takeProfit2)}
                  badge="TP2"
                  badgeClass="bg-emerald-500/15 text-emerald-400 border border-emerald-500/30"
                  tone="success"
                />
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* ============================================================ */}
      {/* CONTEXT ROW — patterns + S/R + outcome tracking                */}
      {/* ============================================================ */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
        <Card>
          <CardContent className="p-4 space-y-2">
            <h3 className="text-[10px] font-semibold tracking-wider text-muted-foreground uppercase">
              Patterns
            </h3>
            {a.detectedPaterns.length > 0 ? (
              <div className="flex flex-wrap gap-1.5">
                {a.detectedPaterns.map((p, i) => (
                  <Badge key={i} variant="secondary" className="text-xs">
                    {p}
                  </Badge>
                ))}
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">None detected.</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4 space-y-2">
            <h3 className="text-[10px] font-semibold tracking-wider text-muted-foreground uppercase">
              Support
            </h3>
            {a.supportLevels.length > 0 ? (
              <div className="flex flex-wrap gap-1.5">
                {a.supportLevels.map((s, i) => (
                  <Badge key={i} variant="outline" className="font-mono text-xs">
                    {fmt(s)}
                  </Badge>
                ))}
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">—</p>
            )}
            <h3 className="text-[10px] font-semibold tracking-wider text-muted-foreground uppercase pt-2">
              Resistance
            </h3>
            {a.resistanceLevels.length > 0 ? (
              <div className="flex flex-wrap gap-1.5">
                {a.resistanceLevels.map((r, i) => (
                  <Badge key={i} variant="outline" className="font-mono text-xs">
                    {fmt(r)}
                  </Badge>
                ))}
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">—</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4 space-y-1.5">
            <h3 className="text-[10px] font-semibold tracking-wider text-muted-foreground uppercase">
              Outcome Tracking
            </h3>
            <OutcomeRow label="TP1 hit" value={a.tp1Hit} />
            <OutcomeRow label="TP2 hit" value={a.tp2Hit} />
            <OutcomeRow label="SL hit" value={a.slHit} negative />
            <div className="flex justify-between text-xs">
              <span className="text-muted-foreground">Resolved</span>
              <span>{fmt(a.resolvedPrice)}</span>
            </div>
            <div className="flex justify-between text-xs">
              <span className="text-muted-foreground">Expires</span>
              <span>{a.expiresAt ? new Date(a.expiresAt).toLocaleDateString() : '—'}</span>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* ============================================================ */}
      {/* AI ANALYSIS CONTENT — long-form, the centerpiece below the fold */}
      {/* ============================================================ */}
      {(a.summary || a.analysis) && (
        <Card>
          <CardContent className="p-6 space-y-4">
            <div className="flex items-center gap-2">
              <div className="size-8 rounded-md bg-[#a855f7]/20 flex items-center justify-center">
                <ArrowRight className="size-4 text-[#a855f7]" />
              </div>
              <h2 className="text-lg font-semibold">AI Analysis</h2>
            </div>

            {a.summary && (
              <div className="rounded-lg border border-[#a855f7]/30 bg-[#a855f7]/5 p-4">
                <p className="text-[10px] font-semibold tracking-wider text-[#a855f7] uppercase mb-1">
                  TL;DR
                </p>
                <p className="text-sm leading-relaxed">{a.summary}</p>
              </div>
            )}

            {a.analysis && (
              <div className="prose-style">
                <h3 className="text-[10px] font-semibold tracking-wider text-muted-foreground uppercase mb-2">
                  Detailed breakdown
                </h3>
                <div className="text-sm leading-relaxed whitespace-pre-wrap text-foreground/90">
                  {a.analysis}
                </div>
              </div>
            )}

            <p className="text-[11px] text-muted-foreground pt-2 border-t border-border">
              Not financial advice. Always do your own research.
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

// ---------------------------------------------------------------------------
// One row in the trade-plan sidebar: label + sub on the left, big price + tag on the right.
// ---------------------------------------------------------------------------
function PlanRow({
  label,
  sub,
  value,
  badge,
  badgeClass,
  tone,
}: {
  label: string
  sub: string
  value: string
  badge: string
  badgeClass: string
  tone?: 'success' | 'danger'
}) {
  const ringClass =
    tone === 'success'
      ? 'border-emerald-500/30 bg-emerald-500/5'
      : tone === 'danger'
      ? 'border-red-500/30 bg-red-500/5'
      : 'border-border bg-card'

  return (
    <div className={`rounded-md border ${ringClass} p-3 flex items-center justify-between gap-3`}>
      <div className="min-w-0">
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="text-lg font-bold font-mono leading-tight">{value}</p>
        <p className="text-[10px] text-muted-foreground">{sub}</p>
      </div>
      <span className={`text-[10px] font-bold px-2 py-1 rounded ${badgeClass}`}>
        {badge}
      </span>
    </div>
  )
}

// ---------------------------------------------------------------------------
function OutcomeRow({
  label,
  value,
  negative,
}: {
  label: string
  value: boolean
  negative?: boolean
}) {
  const color = value
    ? negative
      ? 'text-red-400'
      : 'text-emerald-400'
    : 'text-muted-foreground'
  return (
    <div className="flex justify-between text-xs">
      <span className="text-muted-foreground">{label}</span>
      <span className={`${color} font-medium`}>{value ? 'Yes' : 'No'}</span>
    </div>
  )
}
