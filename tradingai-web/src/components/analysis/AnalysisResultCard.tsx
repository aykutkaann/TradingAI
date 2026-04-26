import { Link } from 'react-router-dom'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { assetUrl } from '@/lib/assetUrl'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { TrendingUp, TrendingDown, Minus } from 'lucide-react'
import { type AnalysisDto, outcomeLabel } from '@/types/analysis'

type Props = {
  analysis: AnalysisDto
}

function TrendBadge({ trend }: { trend: string }) {
  const t = trend.toLowerCase()
  const config =
    t === 'bullish'
      ? { icon: TrendingUp, className: 'bg-green-500/20 text-green-500 border-green-500/40' }
      : t === 'bearish'
      ? { icon: TrendingDown, className: 'bg-red-500/20 text-red-500 border-red-500/40' }
      : { icon: Minus, className: 'bg-muted text-muted-foreground border-border' }
  const Icon = config.icon
  return (
    <Badge variant="outline" className={config.className}>
      <Icon className="size-3 mr-1" />
      {trend}
    </Badge>
  )
}

function StatRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex justify-between py-2 border-b border-border last:border-0">
      <span className="text-muted-foreground">{label}</span>
      <span className="font-medium">{value}</span>
    </div>
  )
}

const fmt = (n: number | null | undefined, digits = 2) =>
  n === null || n === undefined ? '—' : Number(n).toFixed(digits)

export function AnalysisResultCard({ analysis: a }: Props) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between gap-3 flex-wrap">
          <div>
            <CardTitle className="text-2xl">
              {a.assetPair ?? a.assetSymbol ?? 'Image analysis'}{' '}
              <span className="text-muted-foreground text-base">· {a.timeFrame}</span>
            </CardTitle>
            <p className="text-sm text-muted-foreground mt-1">
              by{' '}
              <Link to={`/users/${a.userId}`} className="hover:underline text-foreground">
                {a.userDisplayName}
              </Link>{' '}
              · {new Date(a.createdAt).toLocaleString()}
            </p>
          </div>
          <div className="flex gap-2 flex-wrap">
            <TrendBadge trend={a.trendDirection} />
            <Badge variant={a.outcome === 1 ? 'default' : 'secondary'}>
              {outcomeLabel(a.outcome)}
            </Badge>
            {a.isPublished && <Badge variant="outline">Published</Badge>}
          </div>
        </div>
      </CardHeader>

      <CardContent className="space-y-6">
        {a.chartImageUrl && (
          <img
            src={assetUrl(a.chartImageUrl)}
            alt="Chart"
            className="w-full rounded-md border border-border"
          />
        )}

        {a.summary && (
          <div>
            <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-2">
              Summary
            </h3>
            <p className="text-sm leading-relaxed">{a.summary}</p>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-2">
              Trade Levels
            </h3>
            <div className="text-sm">
              <StatRow label="Entry" value={fmt(a.suggestedEntry)} />
              <StatRow label="Stop Loss" value={fmt(a.stopLoss)} />
              <StatRow label="Take Profit 1" value={fmt(a.takeProfit1)} />
              <StatRow label="Take Profit 2" value={fmt(a.takeProfit2)} />
              <StatRow label="Risk / Reward" value={fmt(a.riskRewardRatio)} />
            </div>
          </div>

          <div>
            <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-2">
              Outcome Tracking
            </h3>
            <div className="text-sm">
              <StatRow label="TP1 Hit" value={a.tp1Hit ? 'Yes' : 'No'} />
              <StatRow label="TP2 Hit" value={a.tp2Hit ? 'Yes' : 'No'} />
              <StatRow label="SL Hit" value={a.slHit ? 'Yes' : 'No'} />
              <StatRow label="Resolved Price" value={fmt(a.resolvedPrice)} />
              <StatRow
                label="Expires"
                value={a.expiresAt ? new Date(a.expiresAt).toLocaleDateString() : '—'}
              />
            </div>
          </div>
        </div>

        {(a.supportLevels.length > 0 || a.resistanceLevels.length > 0) && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-2">
                Support
              </h3>
              <div className="flex flex-wrap gap-2">
                {a.supportLevels.map((s, i) => (
                  <Badge key={i} variant="outline">{fmt(s)}</Badge>
                ))}
              </div>
            </div>
            <div>
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-2">
                Resistance
              </h3>
              <div className="flex flex-wrap gap-2">
                {a.resistanceLevels.map((r, i) => (
                  <Badge key={i} variant="outline">{fmt(r)}</Badge>
                ))}
              </div>
            </div>
          </div>
        )}

        {a.detectedPaterns.length > 0 && (
          <div>
            <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-2">
              Detected Patterns
            </h3>
            <div className="flex flex-wrap gap-2">
              {a.detectedPaterns.map((p, i) => (
                <Badge key={i} variant="secondary">{p}</Badge>
              ))}
            </div>
          </div>
        )}

        {a.analysis && (
          <>
            <Separator />
            <div>
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide mb-2">
                Detailed Analysis
              </h3>
              <p className="text-sm whitespace-pre-wrap leading-relaxed">{a.analysis}</p>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  )
}
