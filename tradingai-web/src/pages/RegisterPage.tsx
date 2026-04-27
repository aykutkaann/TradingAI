import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate, Link } from 'react-router-dom'
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

import { registerSchema, type RegisterFormValues } from '@/features/auth/schemas'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/authStore'

export function RegisterPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((s) => s.setAuth)
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  // Already logged in? Skip the form and go straight to the dashboard.
  useEffect(() => {
    if (isAuthenticated) navigate('/dashboard', { replace: true })
  }, [isAuthenticated, navigate])

  const form = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: '', userName: '', password: '' },
  })

  const registerMutation = useMutation({
    mutationFn: authApi.register,
    onSuccess: (data) => {
      // Validate response structure
      if (!data?.user || !data?.accessToken || !data?.refreshToken) {
        console.error('Invalid registration response:', data)
        toast.error('Invalid server response. Please try again.')
        return
      }

      setAuth(data.user, data.accessToken, data.refreshToken)
      const displayName = data.user.displayName || data.user.userName
      toast.success(`Welcome, ${displayName}!`)

      // Navigate immediately without timeout - state update is synchronous
      navigate('/dashboard', { replace: true })
    },
    onError: (err: any) => {
      console.error('Registration error:', err)

      // Check if user was actually created in DB despite the error
      // This can happen if the error occurred during response transmission
      const hasValidTokens = localStorage.getItem('accessToken') && localStorage.getItem('refreshToken')

      if (hasValidTokens) {
        // Registration likely succeeded but response handling failed
        // Rehydrate auth state from localStorage and navigate
        console.warn('Registration succeeded but response error occurred. Recovering...')
        // The useEffect hook will handle navigation when isAuthenticated becomes true
        // Force a re-check of auth state
        const stored = localStorage.getItem('auth-storage')
        if (stored) {
          try {
            const authState = JSON.parse(stored)
            if (authState.state?.isAuthenticated) {
              toast.success('Registration successful!')
              navigate('/dashboard', { replace: true })
              return
            }
          } catch (e) {
            console.error('Failed to parse auth storage:', e)
          }
        }
      }

      // Handle different error types
      if (err?.code === 'ECONNABORTED') {
        toast.error('Request timeout. Check your backend API URL in .env.production')
      } else if (err?.response?.status === 0) {
        toast.error('Cannot reach backend API. Check VITE_API_URL setting.')
      } else if (err?.response?.status === 409) {
        toast.error('Email or username already exists')
      } else {
        const errorMsg = err?.response?.data?.message ?? err?.message ?? 'Registration failed'
        toast.error(errorMsg)
      }
    },
  })

  const onSubmit = (values: RegisterFormValues) => registerMutation.mutate(values)

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
          <CardTitle className="text-2xl">Create your account</CardTitle>
          <CardDescription>Join Trendox AI to analyze and share trades</CardDescription>
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
                name="userName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Username</FormLabel>
                    <FormControl>
                      <Input placeholder="trader_42" {...field} />
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
              <Button type="submit" className="w-full" disabled={registerMutation.isPending}>
                {registerMutation.isPending ? 'Creating account...' : 'Create account'}
              </Button>
            </form>
          </Form>
          <p className="text-sm text-muted-foreground text-center mt-4">
            Already have an account?{' '}
            <Link to="/login" className="text-primary hover:underline">
              Sign in
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
