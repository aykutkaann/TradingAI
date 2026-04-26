import { Link } from 'react-router-dom'
import logoUrl from '@/assets/logo.png'

/**
 * Marketing footer — only rendered for logged-out visitors (see AppLayout).
 * Social links are placeholders the user will fill in later.
 *
 * Brand icons aren't in our lucide-react version, so we render compact text
 * marks ("IG", "in", "tg", "X") instead. Looks clean against a dark UI and
 * doesn't require an extra icon library.
 */

const SOCIAL_LINKS = [
  { href: '#', label: 'Instagram', mark: 'IG' },
  { href: '#', label: 'LinkedIn', mark: 'in' },
  { href: '#', label: 'Telegram', mark: 'tg' },
  { href: '#', label: 'X (Twitter)', mark: 'X' },
]

export function Footer() {
  return (
    <footer className="border-t border-border bg-card/30 backdrop-blur mt-16">
      <div className="max-w-6xl mx-auto px-4 md:px-8 py-10 grid grid-cols-1 md:grid-cols-3 gap-8">
        {/* Brand + tagline */}
        <div className="space-y-3">
          <Link to="/" className="flex items-center gap-2">
            <img src={logoUrl} alt="" className="h-8 w-8 rounded-md object-cover" />
            <span className="font-bold tracking-tight">
              Trendox<span className="text-[#a855f7] ml-1">AI</span>
            </span>
          </Link>
          <p className="text-sm text-muted-foreground max-w-xs">
            AI-powered trading analysis. Upload a chart, get an instant breakdown.
          </p>
        </div>

        {/* Product links */}
        <div className="space-y-3">
          <h3 className="text-xs font-semibold tracking-wider text-muted-foreground uppercase">
            Product
          </h3>
          <ul className="space-y-2 text-sm">
            <li>
              <Link to="/feed" className="hover:text-primary transition-colors">
                Feed
              </Link>
            </li>
            <li>
              <Link to="/plans" className="hover:text-primary transition-colors">
                Plans
              </Link>
            </li>
            <li>
              <Link to="/login" className="hover:text-primary transition-colors">
                Sign in
              </Link>
            </li>
            <li>
              <Link to="/register" className="hover:text-primary transition-colors">
                Sign up
              </Link>
            </li>
          </ul>
        </div>

        {/* Social */}
        <div className="space-y-3">
          <h3 className="text-xs font-semibold tracking-wider text-muted-foreground uppercase">
            Follow
          </h3>
          <div className="flex items-center gap-2">
            {SOCIAL_LINKS.map((s) => (
              <a
                key={s.label}
                href={s.href}
                target="_blank"
                rel="noopener noreferrer"
                aria-label={s.label}
                title={s.label}
                className="size-9 rounded-md border border-border bg-card flex items-center justify-center text-muted-foreground hover:text-foreground hover:border-[#a855f7]/50 transition-colors text-xs font-bold"
              >
                {s.mark}
              </a>
            ))}
          </div>
          <p className="text-[11px] text-muted-foreground">
            Links are placeholders for now.
          </p>
        </div>
      </div>

      <div className="border-t border-border">
        <div className="max-w-6xl mx-auto px-4 md:px-8 py-4 text-xs text-muted-foreground flex items-center justify-between flex-wrap gap-2">
          <span>© {new Date().getFullYear()} Trendox AI. All rights reserved.</span>
          <span>Not financial advice — for educational purposes only.</span>
        </div>
      </div>
    </footer>
  )
}
