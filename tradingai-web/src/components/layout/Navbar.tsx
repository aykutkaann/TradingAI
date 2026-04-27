import { Link, useNavigate } from 'react-router-dom'
import logoUrl from '@/assets/logo.png'
import { Button } from '@/components/ui/button'

export function Navbar() {
    const navigate = useNavigate()

    const scrollToSection = (sectionId: string) => {
        // If we're not on dashboard, navigate there first
        const element = document.getElementById(sectionId)
        if (element) {
            element.scrollIntoView({ behavior: 'smooth' })
        } else {
            // Navigate to dashboard with a flag to scroll after load
            navigate('/dashboard', { state: { scrollTo: sectionId } })
        }
    }

    return (
        <nav className="sticky top-0 z-50 border-b border-border bg-background/80 backdrop-blur-md">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between items-center h-16">
                    {/* Logo */}
                    <Link to="/" className="flex items-center gap-2 hover:opacity-80 transition-opacity">
                        <img src={logoUrl} alt="Trendox AI" className="h-8 w-8 rounded-md object-cover" />
                        <span className="font-bold text-lg tracking-tight hidden sm:inline">
                            Trendox<span className="text-[#a855f7] ml-1">AI</span>
                        </span>
                    </Link>

                    {/* Center Navigation Links */}
                    <div className="hidden md:flex items-center gap-8">
                        <button
                            onClick={() => document.getElementById('about')?.scrollIntoView({ behavior: 'smooth' })}
                            className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                        >
                            About
                        </button>
                        <button
                            onClick={() => scrollToSection('how-it-works')}
                            className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                        >
                            How it Works
                        </button>
                        <Link
                            to="/plans"
                            className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                        >
                            Pricing
                        </Link>
                    </div>

                    {/* Right side - Auth buttons */}
                    <div className="flex items-center gap-2 sm:gap-3">
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => navigate('/login')}
                            className="text-sm"
                        >
                            Sign In
                        </Button>
                        <Button
                            size="sm"
                            onClick={() => navigate('/register')}
                            className="text-sm"
                        >
                            Sign Up
                        </Button>
                    </div>
                </div>
            </div>
        </nav>
    )
}
