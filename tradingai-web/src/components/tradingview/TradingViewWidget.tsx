import { useEffect, useRef } from 'react'

/**
 * Minimal wrapper around TradingView's embed-widget scripts.
 *
 * TradingView ships a family of widgets (ticker tape, mini chart, symbol
 * overview, full chart, etc.) that are loaded by injecting a <script> tag
 * inside a target <div>. The script reads its config from the JSON inside
 * itself and renders the widget. We just have to (a) build the right URL
 * and (b) re-mount the script if config changes.
 *
 * No external NPM deps — TradingView's CDN handles everything.
 */
type Props = {
  // The script filename. e.g. "embed-widget-ticker-tape.js"
  scriptName: string
  // The JSON config the script reads.
  config: Record<string, unknown>
  className?: string
}

export function TradingViewWidget({ scriptName, config, className }: Props) {
  const containerRef = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    if (!containerRef.current) return
    const container = containerRef.current

    // Clear any previous widget on remount (config change, theme change).
    container.innerHTML =
      '<div class="tradingview-widget-container__widget"></div>'

    const script = document.createElement('script')
    script.type = 'text/javascript'
    script.src = `https://s3.tradingview.com/external-embedding/${scriptName}`
    script.async = true
    script.innerHTML = JSON.stringify(config)
    container.appendChild(script)

    // Cleanup on unmount.
    return () => {
      container.innerHTML = ''
    }
  }, [scriptName, JSON.stringify(config)])

  return (
    <div
      ref={containerRef}
      className={`tradingview-widget-container ${className ?? ''}`}
    />
  )
}
