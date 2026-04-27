import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import {
  Sparkles,
  ArrowRight,
  Plus,
  X,
  CalendarDays,
  History,
} from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useAuthStore } from '@/stores/authStore'
import { watchlistApi } from '@/api/watchlist'
import { assetsApi } from '@/api/assets'
import { analysisApi } from '@/api/analysis'
import { TradingViewWidget } from '@/components/tradingview/TradingViewWidget'
import { AnalysisListCard } from '@/components/analysis/AnalysisListCard'
import {
  toTradingViewSymbol,
  tradingViewChartUrl,
} from '@/lib/tradingViewSymbol'
import type { AssetDto, WatchlistItemDto } from '@/types/analysis'

// Default tickers for the top scrolling tape — used when the user has
// no watchlist yet, and as the inline ticker on every dashboard load.
const DEFAULT_TICKER_SYMBOLS = [
  { proName: 'NASDAQ:QQQ', title: 'QQQ' },
  { proName: 'BINANCE:BTCUSDT', title: 'Bitcoin' },
  { proName: 'BINANCE:ETHUSDT', title: 'Ethereum' },
  { proName: 'FX:EURUSD', title: 'EUR/USD' },
  { proName: 'FX:GBPUSD', title: 'GBP/USD' },
  { proName: 'FOREXCOM:SPXUSD', title: 'S&P 500' },
]

function greetingForNow(): string {
  const h = new Date().getHours()
  if (h < 12) return 'Good morning'
  if (h < 18) return 'Good afternoon'
  return 'Good evening'
}

function formatToday(): string {
  return new Date().toLocaleDateString(undefined, {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
  })
}

export function DashboardPage() {
  const user = useAuthStore((s) => s.user)

  const watchlistQuery = useQuery({
    queryKey: ['watchlist'],
    queryFn: watchlistApi.list,
  })

  // Tracked assets — used (a) as suggestions for an empty watchlist and
  // (b) as the picker in the "+ Add" dropdown so the user can always see
  // what's available.
  const assetsQuery = useQuery({
    queryKey: ['assets'],
    queryFn: () => assetsApi.list(),
  })

  // Last few analyses by the current user — the "pick up where you left off" strip.
  const recentQuery = useQuery({
    queryKey: ['my-analyses', 1],
    queryFn: () => analysisApi.getMine(1, 4),
  })

  return (
    <div className="space-y-8">
      {/* ============================================================ */}
      {/* TOP TICKER TAPE — TradingView's own widget (live, scrolling)   */}
      {/* ============================================================ */}
      <TradingViewWidget
        scriptName="embed-widget-ticker-tape.js"
        config={{
          symbols: DEFAULT_TICKER_SYMBOLS,
          showSymbolLogo: true,
          isTransparent: true,
          displayMode: 'adaptive',
          colorTheme: 'dark',
          locale: 'en',
        }}
        className="rounded-lg overflow-hidden border border-border/50"
      />

      {/* ============================================================ */}
      {/* GREETING ROW                                                   */}
      {/* ============================================================ */}
      <div className="flex items-start justify-between gap-4 flex-wrap">
        <div className="space-y-1">
          <h1 className="text-3xl md:text-4xl font-bold">
            {greetingForNow()}, {user?.displayName ?? user?.userName}!
          </h1>
          <p className="text-muted-foreground flex items-center gap-2">
            <Sparkles className="size-4 text-[#a855f7]" />
            {formatToday()}
          </p>
          <p className="text-sm text-muted-foreground">
            Welcome back. Run a chart through Trendox AI to start your trading journal.
          </p>
        </div>

        <Button
          asChild
          size="lg"
          className="h-12 px-6 text-sm font-semibold rounded-xl bg-gradient-to-r from-[#7c3aed] to-[#3b82f6] hover:opacity-90 shadow-lg shadow-purple-500/30"
        >
          <Link to="/analyze">
            Analyze a Chart <ArrowRight className="size-4 ml-2" />
          </Link>
        </Button>
      </div>

      {/* ============================================================ */}
      {/* MY WATCHLIST                                                   */}
      {/* ============================================================ */}
      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-xs font-semibold tracking-wider text-muted-foreground uppercase">
            My Watchlist
          </h2>
          <AddAssetDropdown
            allAssets={assetsQuery.data ?? []}
            watching={watchlistQuery.data ?? []}
          />
        </div>

        {watchlistQuery.isLoading && (
          <div className="flex gap-3 overflow-x-auto pb-2">
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-44 w-[260px] shrink-0" />
            ))}
          </div>
        )}

        {watchlistQuery.data && watchlistQuery.data.length > 0 && (
          <div className="flex gap-3 overflow-x-auto pb-2">
            {watchlistQuery.data.map((item) => (
              <WatchlistCard key={item.assetId} item={item} />
            ))}
          </div>
        )}

        {watchlistQuery.data &&
          watchlistQuery.data.length === 0 &&
          assetsQuery.data && (
            <EmptyWatchlist suggestions={assetsQuery.data.slice(0, 4)} />
          )}
      </section>

            {/* ============================================================ */}
      {/* MARKET EVENTS — TradingView economic calendar widget           */}
      {/* ============================================================ */}
      <section className="space-y-3">
        <h2 className="text-xs font-semibold tracking-wider text-muted-foreground uppercase flex items-center gap-2">
          <CalendarDays className="size-3.5" /> Today's Market Events
        </h2>
        <div className="rounded-lg overflow-hidden border border-border/50 bg-card/40 backdrop-blur">
          <TradingViewWidget
            scriptName="embed-widget-events.js"
            config={{
              colorTheme: 'dark',
              isTransparent: true,
              width: '100%',
              height: 400,
              locale: 'en',
              importanceFilter: '0,1', // medium + high impact only
            }}
          />
        </div>
      </section>

      {/* ============================================================ */}
      {/* RECENT ANALYSES — last 4, "pick up where you left off"         */}
      {/* ============================================================ */}
      <section className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-xs font-semibold tracking-wider text-muted-foreground uppercase flex items-center gap-2">
            <History className="size-3.5" /> Recent Analyses
          </h2>
          {recentQuery.data && recentQuery.data.totalCount > 4 && (
            <Button asChild variant="ghost" size="sm">
              <Link to="/analyses">
                View all <ArrowRight className="size-3 ml-1" />
              </Link>
            </Button>
          )}
        </div>

        {recentQuery.isLoading && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-32 w-full" />
            ))}
          </div>
        )}

        {recentQuery.data && recentQuery.data.items.length > 0 && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
            {recentQuery.data.items.map((a) => (
              <AnalysisListCard key={a.id} analysis={a} />
            ))}
          </div>
        )}

        {recentQuery.data && recentQuery.data.items.length === 0 && (
          <Card className="border-dashed">
            <CardContent className="p-6 text-center space-y-3">
              <p className="text-sm text-muted-foreground">
                You haven't run any analyses yet.
              </p>
              <Button asChild size="sm">
                <Link to="/analyze">Run your first analysis</Link>
              </Button>
            </CardContent>
          </Card>
        )}
      </section>


    </div>
  )
}

// ---------------------------------------------------------------------------
// One watchlist card — pure TradingView mini widget, nothing layered on top.
// The widget renders its own icon + symbol + name + price + sparkline so we
// don't fight it. We only wrap with a link to tradingview.com and an
// on-hover X to remove from the watchlist.
// ---------------------------------------------------------------------------
function WatchlistCard({ item }: { item: WatchlistItemDto }) {
  const queryClient = useQueryClient()
  const tvSymbol = toTradingViewSymbol(item)

  const removeMutation = useMutation({
    mutationFn: () => watchlistApi.remove(item.assetId),
    onSuccess: () => {
      toast.success(`Removed ${item.pair}`)
      queryClient.invalidateQueries({ queryKey: ['watchlist'] })
    },
    onError: () => toast.error('Could not remove'),
  })

  return (
    <div className="relative group shrink-0 w-[260px]">
      {/* Remove button — visible on hover. Above the widget. */}
      <button
        type="button"
        aria-label={`Remove ${item.pair} from watchlist`}
        onClick={(e) => {
          e.preventDefault()
          e.stopPropagation()
          if (confirm(`Remove ${item.pair} from your watchlist?`)) removeMutation.mutate()
        }}
        disabled={removeMutation.isPending}
        className="absolute top-2 right-2 z-20 size-6 rounded-full bg-background/90 border border-border opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center hover:bg-destructive hover:text-destructive-foreground"
      >
        <X className="size-3" />
      </button>

      {/* Click-through layer — sits above the widget so the whole card opens TV */}
      <a
        href={tradingViewChartUrl(tvSymbol)}
        target="_blank"
        rel="noopener noreferrer"
        aria-label={`Open ${item.pair} on TradingView`}
        className="absolute inset-0 z-10"
      />

      {/* Pure TradingView widget */}
      <div className="rounded-lg overflow-hidden border border-border/50 bg-card/40">
        <TradingViewWidget
          scriptName="embed-widget-mini-symbol-overview.js"
          config={{
            symbol: tvSymbol,
            width: '100%',
            height: 180,
            locale: 'en',
            dateRange: '1D',
            colorTheme: 'dark',
            isTransparent: true,
            autosize: false,
            largeChartUrl: '',
            noTimeScale: true,
            trendLineColor: 'rgba(34, 197, 94, 1)',
            underLineColor: 'rgba(34, 197, 94, 0.15)',
            underLineBottomColor: 'rgba(34, 197, 94, 0)',
          }}
        />
      </div>
    </div>
  )
}

// ---------------------------------------------------------------------------
// "+ Add" dropdown next to the section header. Lists all tracked assets that
// the user is NOT already watching. One-click adds.
// ---------------------------------------------------------------------------
function AddAssetDropdown({
  allAssets,
  watching,
}: {
  allAssets: AssetDto[]
  watching: WatchlistItemDto[]
}) {
  const queryClient = useQueryClient()
  const watchedIds = new Set(watching.map((w) => w.assetId))
  const available = allAssets.filter((a) => !watchedIds.has(a.id))

  const mutation = useMutation({
    mutationFn: (assetId: string) => watchlistApi.add(assetId),
    onSuccess: (_data, assetId) => {
      const asset = allAssets.find((a) => a.id === assetId)
      toast.success(`Added ${asset?.pair ?? 'asset'} to watchlist`)
      queryClient.invalidateQueries({ queryKey: ['watchlist'] })
    },
    onError: () => toast.error('Could not add to watchlist'),
  })

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="outline" size="sm" disabled={available.length === 0}>
          <Plus className="size-3.5 mr-1" />
          Add
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56 max-h-80 overflow-y-auto">
        {available.length === 0 ? (
          <p className="px-3 py-2 text-xs text-muted-foreground">
            All tracked assets are already on your watchlist.
          </p>
        ) : (
          available.map((a) => (
            <DropdownMenuItem
              key={a.id}
              onClick={() => mutation.mutate(a.id)}
              disabled={mutation.isPending}
            >
              <span className="flex-1">{a.pair}</span>
              <span className="text-xs text-muted-foreground ml-2">{a.name}</span>
            </DropdownMenuItem>
          ))
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  )
}

// ---------------------------------------------------------------------------
// Empty watchlist — show suggested popular assets the user can add in one click.
// ---------------------------------------------------------------------------
function EmptyWatchlist({ suggestions }: { suggestions: AssetDto[] }) {
  return (
    <Card className="border-dashed">
      <CardContent className="p-6 text-center space-y-4">
        <p className="text-muted-foreground">
          Your watchlist is empty. Add assets to track their live prices.
        </p>
        <div className="flex flex-wrap justify-center gap-2">
          {suggestions.map((a) => (
            <AddToWatchlistButton key={a.id} asset={a} />
          ))}
        </div>
      </CardContent>
    </Card>
  )
}

function AddToWatchlistButton({ asset }: { asset: AssetDto }) {
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: () => watchlistApi.add(asset.id),
    onSuccess: () => {
      toast.success(`Added ${asset.pair} to watchlist`)
      queryClient.invalidateQueries({ queryKey: ['watchlist'] })
    },
    onError: () => toast.error('Could not add to watchlist'),
  })

  return (
    <Button
      variant="outline"
      size="sm"
      onClick={() => mutation.mutate()}
      disabled={mutation.isPending}
    >
      <Plus className="size-3 mr-1" />
      {asset.pair}
    </Button>
  )
}
