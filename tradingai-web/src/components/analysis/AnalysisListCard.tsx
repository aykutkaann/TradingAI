import { Link } from 'react-router-dom'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { TrendingUp, TrendingDown, Minus, Heart, MessageCircle } from 'lucide-react'
import { type AnalysisDto, outcomeLabel } from '@/types/analysis'

const trendIcon = (trend: string) => {
  const t = trend.toLowerCase()
  if (t === 'bullish') return <TrendingUp className="size-4 text-green-500" />
  if (t === 'bearish') return <TrendingDown className="size-4 text-red-500" />
  return <Minus className="size-4 text-muted-foreground" />
}

const outcomeVariant = (o: number) =>
  o === 1 ? 'default' : o === 2 ? 'destructive' : 'secondary'

export function AnalysisListCard({ analysis: a }: { analysis: AnalysisDto }) {
  return (
    <Link to={`/analyses/${a.id}`}>
      <Card className="hover:border-primary/50 transition-colors cursor-pointer h-full">
        <CardContent className="p-4 space-y-3">
          <div className="flex items-center justify-between gap-2">
            <div className="flex items-center gap-2 min-w-0">
              {trendIcon(a.trendDirection)}
              <span className="font-semibold truncate">
                {a.assetPair ?? a.assetSymbol ?? 'Image'}
              </span>
              <span className="text-xs text-muted-foreground">{a.timeFrame}</span>
            </div>
            <Badge variant={outcomeVariant(a.outcome) as any}>{outcomeLabel(a.outcome)}</Badge>
          </div>

          {a.summary && (
            <p className="text-sm text-muted-foreground line-clamp-2">{a.summary}</p>
          )}

          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span>{new Date(a.createdAt).toLocaleDateString()}</span>
            <div className="flex items-center gap-3">
              <span className="flex items-center gap-1">
                <Heart className="size-3" /> {a.likeCount}
              </span>
              <span className="flex items-center gap-1">
                <MessageCircle className="size-3" /> {a.commentCount}
              </span>
            </div>
          </div>
        </CardContent>
      </Card>
    </Link>
  )
}
