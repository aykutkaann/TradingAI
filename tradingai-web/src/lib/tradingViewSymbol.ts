import { AssetType, type WatchlistItemDto, type AssetDto } from '@/types/analysis'

/**
 * Map an Asset (or watchlist item) to a TradingView symbol like "BINANCE:BTCUSDT".
 * TradingView's widgets need exchange-prefixed symbols.
 *
 * The mapping is heuristic — for production we'd add an explicit
 * `tradingViewSymbol` column to the Asset table and let the seeder fill it.
 */
export function toTradingViewSymbol(asset: Pick<AssetDto | WatchlistItemDto, 'pair' | 'symbol' | 'type'>): string {
  const compact = asset.pair.replace('/', '').toUpperCase()

  switch (asset.type) {
    case AssetType.Crypto:
      return `BINANCE:${compact}` // most crypto pairs trade on Binance
    case AssetType.Forex:
      return `FX:${compact}`
    case AssetType.Stock:
      return `NASDAQ:${asset.symbol.toUpperCase()}` // NASDAQ default; adjust per ticker if needed
    case AssetType.Commodity:
      // Common commodities — fall back to OANDA which TV supports broadly.
      return `OANDA:${compact}`
    default:
      return compact
  }
}

/**
 * Public link to TradingView's chart for a given symbol. Opens in a new tab.
 */
export function tradingViewChartUrl(tvSymbol: string): string {
  return `https://www.tradingview.com/chart/?symbol=${encodeURIComponent(tvSymbol)}`
}
