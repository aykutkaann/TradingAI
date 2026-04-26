import { useState } from 'react'
import { Outlet, Link, NavLink, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { toast } from 'sonner'
import {
  LayoutDashboard,
  LineChart,
  Newspaper,
  Sparkles,
  ListChecks,
  CreditCard,
  Settings,
  LogOut,
  Menu,
  X,
  LifeBuoy,
} from 'lucide-react'

import logoUrl from '@/assets/logo.png'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Badge } from '@/components/ui/badge'
import { useAuthStore } from '@/stores/authStore'
import { NotificationsBell } from '@/components/notifications/NotificationsBell'
import { Footer } from '@/components/layout/Footer'
import { subscriptionsApi } from '@/api/subscriptions'
import { tierLabel } from '@/types/subscription'
import { assetUrl } from '@/lib/assetUrl'

// Items shown in the sidebar's main nav. Each item is a section of the app.
// `requiresAuth` is enforced by ProtectedRoute on the route side; we just hide
// the link in the sidebar to avoid confusion.
const NAV_ITEMS = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard, requiresAuth: true },
  { to: '/analyze', label: 'Analyze', icon: Sparkles, requiresAuth: true },
  { to: '/analyses', label: 'My Analyses', icon: ListChecks, requiresAuth: true },
  { to: '/feed', label: 'Feed', icon: Newspaper, requiresAuth: false },
  { to: '/plans', label: 'Plans', icon: CreditCard, requiresAuth: true },
] as const

export function AppLayout() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  return (
    <div className="min-h-screen bg-background text-foreground flex">
      {/* Desktop sidebar — always visible on md+ */}
      <Sidebar onClose={() => setMobileOpen(false)} className="hidden md:flex" />

      {/* Mobile drawer overlay */}
      {mobileOpen && (
        <>
          <div
            className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm md:hidden"
            onClick={() => setMobileOpen(false)}
            aria-hidden
          />
          <Sidebar
            onClose={() => setMobileOpen(false)}
            className="fixed inset-y-0 left-0 z-50 md:hidden flex"
          />
        </>
      )}

      {/* Main column */}
      <div className="flex-1 min-w-0 flex flex-col">
        {/* Top bar — only visible on mobile (hamburger), and the bell on all sizes */}
        <header className="border-b border-border sticky top-0 z-30 bg-background/80 backdrop-blur">
          <div className="px-4 h-14 flex items-center justify-between gap-2">
            <div className="flex items-center gap-2">
              <Button
                variant="ghost"
                size="sm"
                className="md:hidden"
                onClick={() => setMobileOpen(true)}
                aria-label="Open menu"
              >
                <Menu className="size-5" />
              </Button>
              <Link to="/" className="md:hidden flex items-center gap-2">
                <img src={logoUrl} alt="" className="h-7 w-7 rounded-md object-cover" />
                <span className="font-bold tracking-tight">
                  Trendox<span className="text-[#a855f7] ml-1">AI</span>
                </span>
              </Link>
            </div>
            <div className="flex items-center gap-1">
              <NotificationsBell />
            </div>
          </div>
        </header>

        <main className="flex-1 px-4 md:px-8 py-6">
          <Outlet />
        </main>

        {/* Marketing footer — only for logged-out visitors. Once you're in the
            app you don't need it cluttering the dashboard / analyze flow. */}
        {!isAuthenticated && <Footer />}
      </div>
    </div>
  )
}

// ---------------------------------------------------------------------------
// Sidebar — left rail. Two-mode: persistent on desktop, drawer on mobile.
// Reused for both via different positioning classes from the parent.
// ---------------------------------------------------------------------------
function Sidebar({ onClose, className }: { onClose: () => void; className?: string }) {
  const navigate = useNavigate()
  const user = useAuthStore((s) => s.user)
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const logout = useAuthStore((s) => s.logout)

  const handleLogout = () => {
    logout()
    toast.success('Signed out')
    onClose()
    navigate('/login')
  }

  const visibleNavItems = NAV_ITEMS.filter((item) => !item.requiresAuth || isAuthenticated)

  return (
    <aside
      // Pin the sidebar to the viewport so the user/plan/logout footer stays
      // visible when the main column scrolls.
      // - md+ : sticky, h = full screen, top:0 → stays put as you scroll content
      // - mobile drawer : already fixed inset-y-0 from the parent's className
      className={`w-60 shrink-0 border-r border-border bg-card/40 backdrop-blur flex-col md:sticky md:top-0 md:h-screen ${className ?? ''}`}
    >
      {/* Brand */}
      <div className="h-16 px-4 border-b border-border flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2" onClick={onClose}>
          <img src={logoUrl} alt="" className="h-8 w-8 rounded-md object-cover" />
          <span className="text-lg font-bold tracking-tight">
            Trendox<span className="text-[#a855f7] ml-1">AI</span>
          </span>
        </Link>
        <Button
          variant="ghost"
          size="sm"
          className="md:hidden"
          onClick={onClose}
          aria-label="Close menu"
        >
          <X className="size-4" />
        </Button>
      </div>

      {/* Primary nav */}
      <nav className="flex-1 overflow-y-auto py-4 space-y-1 px-3">
        <p className="px-3 pb-2 text-[11px] font-semibold tracking-wider text-muted-foreground uppercase">
          Trading Tools
        </p>
        {visibleNavItems.map((item) => {
          const Icon = item.icon
          return (
            <NavLink
              key={item.to}
              to={item.to}
              onClick={onClose}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2 rounded-md text-sm transition-colors ${
                  isActive
                    ? 'bg-[#a855f7]/15 text-[#a855f7] font-medium'
                    : 'text-muted-foreground hover:text-foreground hover:bg-accent'
                }`
              }
            >
              <Icon className="size-4 shrink-0" />
              {item.label}
            </NavLink>
          )
        })}

        {/* Help & Support — auth-only, lives below the main nav */}
        {isAuthenticated && (
          <>
            <p className="px-3 pt-6 pb-2 text-[11px] font-semibold tracking-wider text-muted-foreground uppercase">
              Help &amp; Support
            </p>
            <NavLink
              to="/support"
              onClick={onClose}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2 rounded-md text-sm transition-colors ${
                  isActive
                    ? 'bg-[#a855f7]/15 text-[#a855f7] font-medium'
                    : 'text-muted-foreground hover:text-foreground hover:bg-accent'
                }`
              }
            >
              <LifeBuoy className="size-4 shrink-0" />
              Support
            </NavLink>
          </>
        )}
      </nav>

      {/* Footer — auth-aware */}
      <div className="border-t border-border p-3 space-y-2">
        {isAuthenticated && user ? (
          <>
            <PlanLine />
            <div className="flex items-center gap-2 px-1">
              <Avatar className="size-8">
                {user.avatarUrl ? (
                  <AvatarImage src={assetUrl(user.avatarUrl)} alt={user.userName} />
                ) : null}
                <AvatarFallback className="text-xs">
                  {(user.displayName ?? user.userName).slice(0, 2).toUpperCase()}
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">
                  {user.displayName ?? user.userName}
                </p>
                <p className="text-xs text-muted-foreground truncate">{user.email}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-2 pt-1">
              <Button asChild variant="ghost" size="sm" className="justify-start">
                <Link to="/settings" onClick={onClose}>
                  <Settings className="size-4 mr-1" /> Settings
                </Link>
              </Button>
              <Button
                variant="ghost"
                size="sm"
                className="justify-start text-destructive hover:text-destructive"
                onClick={handleLogout}
              >
                <LogOut className="size-4 mr-1" /> Logout
              </Button>
            </div>
          </>
        ) : (
          <div className="space-y-2">
            <Button asChild className="w-full" onClick={onClose}>
              <Link to="/login">Sign in</Link>
            </Button>
            <Button asChild variant="outline" className="w-full" onClick={onClose}>
              <Link to="/register">Sign up</Link>
            </Button>
          </div>
        )}
      </div>
    </aside>
  )
}

// Subscription pill in the sidebar footer.
function PlanLine() {
  const { data } = useQuery({
    queryKey: ['my-subscription'],
    queryFn: subscriptionsApi.getMine,
    staleTime: 60_000,
  })
  if (!data) return null
  return (
    <div className="px-1 pb-1 flex items-center gap-2">
      <LineChart className="size-3 text-muted-foreground" />
      <Badge variant={data.tier === 0 ? 'secondary' : 'default'} className="text-[10px] px-1.5 py-0">
        {tierLabel(data.tier)} plan
      </Badge>
    </div>
  )
}
