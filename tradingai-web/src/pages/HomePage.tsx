import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import {
  LineChart,
  ImageIcon,
  Sparkles,
  Target,
  Users,
  Zap,
  Check,
  ArrowRight,
  PlayCircle,
} from 'lucide-react'
import { useAuthStore } from '@/stores/authStore'

const features = [
  {
    icon: LineChart,
    title: 'Tracked assets',
    body: 'Pick from BTC, ETH, EUR/USD and more. AI fetches live data and returns a full breakdown in seconds.',
  },
  {
    icon: ImageIcon,
    title: 'Image upload',
    body: 'Drop a screenshot from TradingView, Binance, MT4 — anywhere. AI reads the chart directly.',
  },
  {
    icon: Sparkles,
    title: 'Trade ideas',
    body: 'Trend, support / resistance, entry / SL / TP levels and a written analysis — every time.',
  },
  {
    icon: Target,
    title: 'Outcome tracking',
    body: 'We watch the market for you. When TP or SL hits, you get a notification — and an email on wins.',
  },
  {
    icon: Users,
    title: 'Social feed',
    body: 'Follow traders whose calls land. Like, comment, build a reputation on the public feed.',
  },
  {
    icon: Zap,
    title: 'Instant',
    body: 'No screens to watch, no indicators to set up. Paste a chart, pick a timeframe, hit analyze.',
  },
]

const trialPerks = [
  'Get your first 3 analyses free',
  'Both image upload AND tracked assets',
  'Full social features — feed, comments, follows',
  'No credit card required',
]

const stats = [
  { value: '50K+', label: 'Active Traders' },
  { value: '1M+', label: 'Charts Analyzed' },
  { value: '97%', label: 'Satisfaction' },
]

const howItWorksSteps = [
  {
    n: 1,
    title: 'Screenshot Your Chart',
    body: 'Snap any chart from TradingView, your broker, or any platform.',
    video: '/step-1.mp4',
    highlight: false,
  },
  {
    n: 2,
    title: 'AI Analyzes Your Chart',
    body: 'AI scans for patterns, support/resistance, and market structure in seconds.',
    video: '/step-2.mp4',
    highlight: false,
  },
  {
    n: 3,
    title: 'Get Your Trade Plan',
    body: 'Receive entry, exit, SL/TP levels with confidence scores and R:R ratios.',
    video: '/step-3.mp4',
    highlight: false,
  },
  {
    n: 4,
    title: 'Trade With Confidence',
    body: "Use the AI's recommendation alongside your own analysis — it sharpens your edge, not replaces your judgment.",
    video: '/step-4.mp4',
    highlight: true,
  },
]

export function HomePage() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  return (
    <div className="space-y-16 pb-12">
      {/* ============================================================ */}
      {/* HERO — two columns: copy + CTAs left, demo video right         */}
      {/* The radial purple glow is the layout's mood-setter; everything */}
      {/* else stays close to your existing dark theme.                  */}
      {/* ============================================================ */}
      <section className="relative -mx-4 px-4 pt-8 pb-10 overflow-hidden">
        {/* Purple radial glow background */}
        <div
          className="absolute inset-0 -z-10 pointer-events-none"
          style={{
            background:
              'radial-gradient(ellipse 80% 60% at 30% 30%, rgba(139, 92, 246, 0.18), transparent 60%), radial-gradient(ellipse 60% 50% at 80% 70%, rgba(168, 85, 247, 0.12), transparent 60%)',
          }}
          aria-hidden
        />

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-6 items-center">
          {/* Left — copy + CTAs */}
          <div className="space-y-6">
            <h1 className="text-4xl md:text-5xl lg:text-6xl font-extrabold tracking-tight leading-[1.05]">
              Trade with{' '}
              <span className="text-[#a855f7]">AI precision</span>
              <br />
              not gut feelings.
            </h1>

            <p className="text-base md:text-lg text-muted-foreground max-w-xl">
              Upload your chart, get instant AI analysis with precise entries,
              exits, and risk management.
            </p>

            <div className="flex flex-wrap gap-3">
              <Button
                asChild
                size="lg"
                className="h-12 px-6 text-sm font-semibold rounded-xl bg-gradient-to-r from-[#7c3aed] to-[#3b82f6] hover:opacity-90 transition-opacity shadow-lg shadow-purple-500/30"
              >
                <Link to={isAuthenticated ? '/analyze' : '/register'}>
                  <Target className="size-4 mr-2" />
                  Start Analyzing
                </Link>
              </Button>

              <Button
                asChild
                size="lg"
                variant="outline"
                className="h-12 px-6 text-sm font-semibold rounded-xl border-border/60 bg-background/40 backdrop-blur"
              >
                <a href="#demo">
                  <PlayCircle className="size-4 mr-2" />
                  Watch Demo
                </a>
              </Button>
            </div>
          </div>

          {/* Right — looping demo video, framed in a dark rounded card */}
          <div id="demo" className="relative">
            <div className="rounded-2xl overflow-hidden border border-border/50 bg-card/50 backdrop-blur shadow-2xl shadow-purple-500/10">
              <video
                src="/hero-demo.mp4"
                autoPlay
                muted
                loop
                playsInline
                className="w-full h-auto block"
              />
            </div>
            {/* Soft glow behind the video */}
            <div
              className="absolute inset-0 -z-10 blur-3xl opacity-40 pointer-events-none"
              style={{
                background:
                  'radial-gradient(ellipse at center, rgba(139, 92, 246, 0.5), transparent 70%)',
              }}
              aria-hidden
            />
          </div>
        </div>

        {/* Stats row */}
        <div className="mt-12 flex items-center justify-center gap-6 md:gap-12 flex-wrap text-center">
          {stats.map((s, i) => (
            <div key={s.label} className="flex items-center gap-3">
              <div>
                <div className="text-xl md:text-2xl font-bold">{s.value}</div>
                <div className="text-xs text-muted-foreground">{s.label}</div>
              </div>
              {i < stats.length - 1 && (
                <div className="hidden md:block h-8 w-px bg-border ml-6" />
              )}
            </div>
          ))}
        </div>
      </section>

      {/* ============================================================ */}
      {/* HOW IT WORKS — 4 steps in a 2x2 grid, each card with a clip    */}
      {/* ============================================================ */}
      <section className="max-w-5xl mx-auto space-y-8">
        <div className="text-center space-y-1">
          <h2 className="text-2xl md:text-4xl font-bold">How It Works</h2>
          <p className="text-muted-foreground">
            See Trendox AI in action — from screenshot to trade plan in seconds.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {howItWorksSteps.map((step) => (
            <div
              key={step.n}
              className={`relative rounded-xl border bg-card/40 backdrop-blur p-3 transition-colors ${
                step.highlight
                  ? 'border-[#a855f7]/60 shadow-lg shadow-purple-500/20'
                  : 'border-border/50'
              }`}
            >
              {/* Clip — falls back to the full hero video if step-N.mp4 isn't present yet */}
              <div className="rounded-md overflow-hidden border border-border/50 bg-black">
                <video
                  src={step.video}
                  autoPlay
                  muted
                  loop
                  playsInline
                  onError={(e) => {
                    // If step-N.mp4 doesn't exist yet, fall back to the hero clip
                    // so the card still shows something while you produce the 4 cuts.
                    const v = e.currentTarget
                    if (!v.dataset.fellBack) {
                      v.dataset.fellBack = '1'
                      v.src = '/hero-demo.mp4'
                    }
                  }}
                  className="w-full h-auto block"
                />
              </div>

              <div className="flex items-start gap-2 pt-3">
                <div className="size-6 rounded-full bg-[#a855f7]/20 text-[#a855f7] flex items-center justify-center text-xs font-bold shrink-0">
                  {step.n}
                </div>
                <div className="space-y-0.5">
                  <h3 className="font-semibold text-sm">{step.title}</h3>
                  <p className="text-xs text-muted-foreground">{step.body}</p>
                </div>
              </div>
            </div>
          ))}
        </div>

        <div className="text-center">
          <Button
            asChild
            size="lg"
            className="h-12 px-6 text-sm font-semibold rounded-xl bg-gradient-to-r from-[#7c3aed] to-[#3b82f6] hover:opacity-90 shadow-lg shadow-purple-500/30"
          >
            <Link to={isAuthenticated ? '/analyze' : '/register'}>
              Try It Yourself <ArrowRight className="size-4 ml-1" />
            </Link>
          </Button>
        </div>
      </section>

      {/* ============================================================ */}
      {/* TRIAL CARD — only for logged-out visitors                      */}
      {/* ============================================================ */}
      {!isAuthenticated && (
        <section className="max-w-3xl mx-auto">
          <Card className="border-[#a855f7]/30 bg-gradient-to-br from-[#a855f7]/10 to-transparent">
            <CardContent className="p-8 space-y-4">
              <Badge variant="outline" className="text-[#a855f7] border-[#a855f7]/40">
                <Sparkles className="size-9 mr-1" /> Free trial. No credit card required.
              </Badge>
              <h2 className="text-2xl font-bold">Try every feature free</h2>
              <ul className="space-y-2">
                {trialPerks.map((perk) => (
                  <li key={perk} className="flex items-start gap-2 text-sm">
                    <Check className="size-4 text-[#a855f7] mt-0.5 shrink-0" />
                    <span>{perk}</span>
                  </li>
                ))}
              </ul>
              <div className="pt-2">
                <Button asChild size="lg">
                  <Link to="/register">Create free account</Link>
                </Button>
              </div>
            </CardContent>
          </Card>
        </section>
      )}

      {/* ============================================================ */}
      {/* FEATURE GRID                                                  */}
      {/* ============================================================ */}
      <section className="max-w-5xl mx-auto space-y-6">
        <div className="text-center space-y-2">
          <h2 className="text-3xl md:text-4xl font-bold">
            Everything you need to trade smarter
          </h2>
          <p className="text-muted-foreground">
            One platform for analysis, tracking and learning from a community of traders.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {features.map((f) => {
            const Icon = f.icon
            return (
              <Card key={f.title}>
                <CardContent className="p-6 space-y-3">
                  <Icon className="size-8 text-[#a855f7]" />
                  <h3 className="font-semibold text-lg">{f.title}</h3>
                  <p className="text-sm text-muted-foreground">{f.body}</p>
                </CardContent>
              </Card>
            )
          })}
        </div>
      </section>

      {/* ============================================================ */}
      {/* BOTTOM CTA                                                    */}
      {/* ============================================================ */}
      {!isAuthenticated && (
        <section className="text-center space-y-4 max-w-2xl mx-auto py-8">
          <h2 className="text-3xl md:text-4xl font-bold">Ready to see your edge?</h2>
          <p className="text-muted-foreground">
            Join Trendox AI today. First analysis takes about 30 seconds.
          </p>
          <Button
            asChild
            size="lg"
            className="h-14 px-8 text-base font-semibold rounded-xl bg-gradient-to-r from-[#7c3aed] to-[#3b82f6] hover:opacity-90 shadow-lg shadow-purple-500/30"
          >
            <Link to="/register">
              Start free trial <ArrowRight className="size-4 ml-1" />
            </Link>
          </Button>
        </section>
      )}
    </div>
  )
}
