/**
 * Backend stores uploaded files at relative paths like
 *   /uploads/charts/{userId}/{guid}.png
 * The browser would resolve those against the frontend origin (Vite on :5173)
 * which doesn't have them — they're served by ASP.NET on :7155 (or the
 * deployed API URL).
 *
 * This helper prepends VITE_API_URL when the path looks relative.
 * Pass through anything that already has a scheme (`http://`, `https://`,
 * `data:`, `blob:`) untouched.
 */
const API_BASE = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

export function assetUrl(path: string | null | undefined): string | undefined {
  if (!path) return undefined
  if (/^(https?:|data:|blob:)/i.test(path)) return path
  if (!path.startsWith('/')) return `${API_BASE}/${path}`
  return `${API_BASE}${path}`
}
