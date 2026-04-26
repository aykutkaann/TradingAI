import { useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery } from '@tanstack/react-query'
import { toast } from 'sonner'
import {
  User as UserIcon,
  Trophy,
  CreditCard,
  Shield,
  Upload,
  Award,
  ArrowRight,
} from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Switch } from '@/components/ui/switch'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useAuthStore } from '@/stores/authStore'
import { subscriptionsApi } from '@/api/subscriptions'
import { tierLabel } from '@/types/subscription'
import { usersApi } from '@/api/users'
import { getApiErrorMessage } from '@/lib/errors'
import { assetUrl } from '@/lib/assetUrl'

const TIMEZONES = [
  'UTC',
  'Eastern Time (ET)',
  'Central Time (CT)',
  'Mountain Time (MT)',
  'Pacific Time (PT)',
  'Europe/London',
  'Europe/Berlin',
  'Europe/Istanbul',
  'Asia/Tokyo',
  'Asia/Singapore',
]

export function SettingsPage() {
  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-3xl font-bold">Settings</h1>
        <p className="text-muted-foreground">
          Manage your account and preferences
        </p>
      </div>

      <Tabs defaultValue="profile" className="w-full">
        <TabsList className="grid grid-cols-4 max-w-2xl">
          <TabsTrigger value="profile">
            <UserIcon className="size-4 mr-1" /> Profile
          </TabsTrigger>
          <TabsTrigger value="badges">
            <Trophy className="size-4 mr-1" /> Badges
          </TabsTrigger>
          <TabsTrigger value="billing">
            <CreditCard className="size-4 mr-1" /> Billing
          </TabsTrigger>
          <TabsTrigger value="security">
            <Shield className="size-4 mr-1" /> Security
          </TabsTrigger>
        </TabsList>

        <TabsContent value="profile" className="mt-6">
          <ProfileTab />
        </TabsContent>
        <TabsContent value="badges" className="mt-6">
          <BadgesTab />
        </TabsContent>
        <TabsContent value="billing" className="mt-6">
          <BillingTab />
        </TabsContent>
        <TabsContent value="security" className="mt-6">
          <SecurityTab />
        </TabsContent>
      </Tabs>
    </div>
  )
}

// ===========================================================================
// PROFILE
// ===========================================================================
function ProfileTab() {
  const user = useAuthStore((s) => s.user)
  const setUser = useAuthStore((s) => s.setUser)

  const [displayName, setDisplayName] = useState(user?.displayName ?? user?.userName ?? '')
  // Email change requires a re-verification flow we haven't built — read-only for now.
  const [email] = useState(user?.email ?? '')
  // Timezone is local-only (no DB column). Persist in localStorage so it sticks.
  const [timezone, setTimezone] = useState(
    () => localStorage.getItem('user-timezone') ?? 'UTC'
  )
  const [bio, setBio] = useState('')

  const fileInputRef = useRef<HTMLInputElement | null>(null)
  const initials = (displayName || 'A').slice(0, 1).toUpperCase()

  const saveMutation = useMutation({
    mutationFn: () => usersApi.updateMe({ displayName, bio }),
    onSuccess: (updated) => {
      setUser(updated)
      localStorage.setItem('user-timezone', timezone)
      toast.success('Profile saved')
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Save failed')),
  })

  const avatarMutation = useMutation({
    mutationFn: (file: File) => usersApi.uploadAvatar(file),
    onSuccess: (updated) => {
      setUser(updated)
      toast.success('Avatar updated')
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Upload failed')),
  })

  const handleCancel = () => {
    setDisplayName(user?.displayName ?? user?.userName ?? '')
    setBio('')
    setTimezone(localStorage.getItem('user-timezone') ?? 'UTC')
  }

  const handleFilePick = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0]
    if (!f) return
    if (f.size > 1024 * 1024) {
      toast.error('Image must be 1 MB or less')
      return
    }
    avatarMutation.mutate(f)
    // reset so picking the same file again still triggers onChange
    e.target.value = ''
  }

  return (
    <Card>
      <CardContent className="p-6 space-y-6">
        <div className="space-y-1">
          <h2 className="text-xl font-semibold">Profile Settings</h2>
          <p className="text-sm text-muted-foreground">
            Update your personal information and preferences
          </p>
        </div>

        {/* Avatar row */}
        <div className="flex items-center gap-4">
          <Avatar className="size-16 bg-[#a855f7]">
            {user?.avatarUrl ? (
              <AvatarImage src={assetUrl(user.avatarUrl)} alt={user.userName} />
            ) : null}
            <AvatarFallback className="text-2xl bg-[#a855f7] text-white">
              {initials}
            </AvatarFallback>
          </Avatar>
          <div className="space-y-1">
            <input
              ref={fileInputRef}
              type="file"
              accept="image/png,image/jpeg,image/webp,image/gif"
              className="hidden"
              onChange={handleFilePick}
            />
            <Button
              variant="outline"
              size="sm"
              onClick={() => fileInputRef.current?.click()}
              disabled={avatarMutation.isPending}
            >
              <Upload className="size-4 mr-1" />
              {avatarMutation.isPending ? 'Uploading…' : 'Upload Photo'}
            </Button>
            <p className="text-xs text-muted-foreground">JPG, GIF or PNG. 1 MB max.</p>
          </div>
        </div>

        {/* Two-column form */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <Label className="text-xs">Display Name</Label>
            <Input
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="Your full name"
            />
          </div>
          <div className="space-y-1.5">
            <Label className="text-xs">Email Address</Label>
            <Input type="email" value={email} disabled />
            <p className="text-[11px] text-muted-foreground">
              Email change requires verification — coming soon.
            </p>
          </div>
          <div className="space-y-1.5">
            <Label className="text-xs">Timezone</Label>
            <Select value={timezone} onValueChange={setTimezone}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {TIMEZONES.map((tz) => (
                  <SelectItem key={tz} value={tz}>
                    {tz}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1.5">
            <Label className="text-xs">Trading Bio</Label>
            <Input
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              placeholder="e.g. Day trader focused on tech stocks"
              maxLength={140}
            />
          </div>
        </div>

        <div className="flex items-center justify-between pt-2">
          <Button variant="outline" onClick={handleCancel} disabled={saveMutation.isPending}>
            Cancel
          </Button>
          <Button
            className="bg-[#a855f7] hover:bg-[#9333ea]"
            onClick={() => saveMutation.mutate()}
            disabled={saveMutation.isPending}
          >
            {saveMutation.isPending ? 'Saving…' : 'Save Changes'}
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}

// ===========================================================================
// BADGES — placeholder until we build a real achievement system
// ===========================================================================
function BadgesTab() {
  return (
    <Card>
      <CardContent className="p-6 space-y-4">
        <div className="space-y-1">
          <h2 className="text-xl font-semibold flex items-center gap-2">
            <Trophy className="size-5 text-[#a855f7]" /> Badges
          </h2>
          <p className="text-sm text-muted-foreground">
            Earn badges by analyzing charts, hitting take-profits, and growing your following.
          </p>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          {['First Analysis', 'First Win', 'Streak x5', 'Top Trader'].map((b) => (
            <div
              key={b}
              className="rounded-lg border border-dashed border-border p-4 flex flex-col items-center text-center gap-2 opacity-60"
            >
              <Award className="size-8 text-muted-foreground" />
              <p className="text-xs">{b}</p>
              <span className="text-[10px] text-muted-foreground">Locked</span>
            </div>
          ))}
        </div>

        <p className="text-xs text-muted-foreground text-center pt-2">
          Badge system coming soon.
        </p>
      </CardContent>
    </Card>
  )
}

// ===========================================================================
// BILLING — current plan + upgrade CTA + (placeholder) payment methods / history
// ===========================================================================
function BillingTab() {
  const { data, isLoading } = useQuery({
    queryKey: ['my-subscription'],
    queryFn: subscriptionsApi.getMine,
  })

  return (
    <Card>
      <CardContent className="p-6 space-y-6">
        <div className="space-y-1">
          <h2 className="text-xl font-semibold">Billing Information</h2>
          <p className="text-sm text-muted-foreground">
            Manage your subscription and payment methods
          </p>
        </div>

        {/* Current plan */}
        {isLoading ? (
          <Skeleton className="h-16 w-full" />
        ) : (
          <div className="rounded-lg border border-border bg-card/40 p-4 flex items-center justify-between">
            <div>
              <p className="font-semibold">Current Plan</p>
              <p className="text-sm text-muted-foreground">
                {data ? `${tierLabel(data.tier)} Tier` : 'Free Tier'}
                {data?.expiresAt && (
                  <span className="ml-2">
                    · expires {new Date(data.expiresAt).toLocaleDateString()}
                  </span>
                )}
              </p>
            </div>
            <Button asChild variant="outline">
              <Link to="/plans">
                Upgrade <ArrowRight className="size-3 ml-1" />
              </Link>
            </Button>
          </div>
        )}

        {/* Payment methods placeholder */}
        <div className="space-y-2">
          <h3 className="font-semibold">Payment Methods</h3>
          <p className="text-sm text-muted-foreground">No payment methods added</p>
        </div>

        {/* Billing history placeholder */}
        <div className="space-y-2">
          <h3 className="font-semibold">Billing History</h3>
          <p className="text-sm text-muted-foreground">No billing history</p>
        </div>
      </CardContent>
    </Card>
  )
}

// ===========================================================================
// SECURITY — change password + 2FA toggle + session timeout
// ===========================================================================
function SecurityTab() {
  const [current, setCurrent] = useState('')
  const [next, setNext] = useState('')
  const [confirm, setConfirm] = useState('')
  const [twoFactor, setTwoFactor] = useState(false)
  const [sessionTimeout, setSessionTimeout] = useState(true)

  const changePasswordMutation = useMutation({
    mutationFn: () =>
      usersApi.changePassword({ currentPassword: current, newPassword: next }),
    onSuccess: () => {
      toast.success('Password updated')
      setCurrent('')
      setNext('')
      setConfirm('')
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Could not update password')),
  })

  const handleUpdate = () => {
    if (!current || !next || !confirm) {
      toast.error('Fill in all password fields')
      return
    }
    if (next !== confirm) {
      toast.error('New passwords do not match')
      return
    }
    if (next.length < 8) {
      toast.error('Password must be at least 8 characters')
      return
    }
    changePasswordMutation.mutate()
  }

  return (
    <Card>
      <CardContent className="p-6 space-y-6">
        <div className="space-y-1">
          <h2 className="text-xl font-semibold">Security Settings</h2>
          <p className="text-sm text-muted-foreground">Manage your account security</p>
        </div>

        <div className="space-y-4">
          <div className="space-y-1.5">
            <Label className="text-xs">Current Password</Label>
            <Input
              type="password"
              value={current}
              onChange={(e) => setCurrent(e.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label className="text-xs">New Password</Label>
            <Input
              type="password"
              value={next}
              onChange={(e) => setNext(e.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label className="text-xs">Confirm New Password</Label>
            <Input
              type="password"
              value={confirm}
              onChange={(e) => setConfirm(e.target.value)}
            />
          </div>
        </div>

        {/* Toggles */}
        <div className="space-y-3 pt-2">
          <ToggleRow
            title="Two-Factor Authentication"
            description="Add an extra layer of security"
            checked={twoFactor}
            onChange={setTwoFactor}
          />
          <ToggleRow
            title="Session Timeout"
            description="Automatically log out after inactivity"
            checked={sessionTimeout}
            onChange={setSessionTimeout}
          />
        </div>

        <Button
          className="bg-[#a855f7] hover:bg-[#9333ea]"
          onClick={handleUpdate}
          disabled={changePasswordMutation.isPending}
        >
          {changePasswordMutation.isPending ? 'Updating…' : 'Update Security'}
        </Button>
        <p className="text-[11px] text-muted-foreground">
          Two-factor authentication and session timeout are visual only for now.
        </p>
      </CardContent>
    </Card>
  )
}

function ToggleRow({
  title,
  description,
  checked,
  onChange,
}: {
  title: string
  description: string
  checked: boolean
  onChange: (v: boolean) => void
}) {
  return (
    <div className="flex items-center justify-between gap-3">
      <div className="min-w-0">
        <p className="text-sm font-medium">{title}</p>
        <p className="text-xs text-muted-foreground">{description}</p>
      </div>
      <Switch checked={checked} onCheckedChange={onChange} />
    </div>
  )
}

