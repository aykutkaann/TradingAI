import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import logoUrl from '@/assets/logo.png'

import { loginSchema, type LoginFormValues } from '@/features/auth/schemas'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/authStore'

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const setAuth = useAuthStore((s) => s.setAuth)
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  // If the user is already signed in, don't show this page at all —
  // bounce straight to the dashboard.
  useEffect(() => {
    if (isAuthenticated) navigate('/dashboard', { replace: true })
  }, [isAuthenticated, navigate])

  // If the user was bounced here by ProtectedRoute, send them back where
  // they came from after sign-in. Otherwise default to /dashboard.
  const from = (location.state as { from?: { pathname: string } } | null)?.from?.pathname ?? '/dashboard'

  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  const loginMutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: (data) => {
      try {
        // Validate response structure
        if (!data.user || !data.accessToken || !data.refreshToken) {
          console.error('Invalid login response:', data)
          toast.error('Login response invalid. Please try again.')
          return
        }

        setAuth(data.user, data.accessToken, data.refreshToken)
        const displayName = data.user.displayName || data.user.userName
        toast.success(`Welcome back, ${displayName}!`)

        // Use setTimeout to ensure state updates propagate before navigation
        setTimeout(() => {
          navigate(from, { replace: true })
        }, 100)
      } catch (error) {
        console.error('Error processing login:', error)
        toast.error('An error occurred. Please try again.')
      }
    },
    onError: (err: any) => {
      const errorMsg = err?.response?.data?.message ?? err?.message ?? 'Login failed'
      toast.error(errorMsg)
      console.error('Login error:', err)
    },
  })

  const onSubmit = (values: LoginFormValues) => loginMutation.mutate(values)

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-4 bg-background gap-6">
      {/* Brand row above the card. Logged-in users get sent to the dashboard
          (the useEffect above also auto-redirects them on mount).
          Logged-out users get the marketing home so they don't bounce back here. */}
      <Link
        to={isAuthenticated ? '/dashboard' : '/'}
        className="flex items-center gap-3 hover:opacity-90 transition-opacity"
      >
        <img src={logoUrl} alt="" className="h-12 w-12 rounded-md object-cover" />
        <span className="text-3xl font-bold tracking-tight">
          Trendox<span className="text-[#a855f7] ml-1">AI</span>
        </span>
      </Link>

      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">Welcome back</CardTitle>
          <CardDescription>Enter your credentials to access your account</CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email</FormLabel>
                    <FormControl>
                      <Input type="email" placeholder="you@example.com" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Password</FormLabel>
                    <FormControl>
                      <Input type="password" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <Button type="submit" className="w-full" disabled={loginMutation.isPending}>
                {loginMutation.isPending ? 'Signing in...' : 'Sign in'}
              </Button>
            </form>
          </Form>
          <p className="text-sm text-muted-foreground text-center mt-4">
            Don't have an account?{' '}
            <Link to="/register" className="text-primary hover:underline">
              Sign up
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  )
}