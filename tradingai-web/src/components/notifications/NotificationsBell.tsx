import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Bell, Check, UserPlus, Heart, MessageCircle, Target } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { notificationsApi } from '@/api/notifications'
import { type NotificationDto, NotificationType } from '@/types/notification'
import { useAuthStore } from '@/stores/authStore'

// Poll every 30s while the bell is mounted. Cheap enough; React Query
// dedupes if the panel is also open and triggers its own fetch.
const POLL_INTERVAL_MS = 30_000

export function NotificationsBell() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const [open, setOpen] = useState(false)
  const queryClient = useQueryClient()

  // Unread count: polled in the background.
  const unreadQuery = useQuery({
    queryKey: ['notifications-unread'],
    queryFn: notificationsApi.unreadCount,
    enabled: isAuthenticated,
    refetchInterval: POLL_INTERVAL_MS,
    refetchIntervalInBackground: false,
  })

  // Full list: only fetch when the dropdown is open.
  const listQuery = useQuery({
    queryKey: ['notifications-list'],
    queryFn: () => notificationsApi.list(1, 20),
    enabled: isAuthenticated && open,
  })

  const markAllMutation = useMutation({
    mutationFn: notificationsApi.markAllAsRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications-unread'] })
      queryClient.invalidateQueries({ queryKey: ['notifications-list'] })
      toast.success('All caught up')
    },
  })

  if (!isAuthenticated) return null

  const unread = unreadQuery.data ?? 0

  return (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="sm" className="relative" aria-label="Notifications">
          <Bell className="size-5" />
          {unread > 0 && (
            <Badge
              variant="default"
              className="absolute -top-1 -right-1 size-5 p-0 flex items-center justify-center text-xs"
            >
              {unread > 99 ? '99+' : unread}
            </Badge>
          )}
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" className="w-80 sm:w-96 p-0">
        <div className="flex items-center justify-between px-4 py-3 border-b border-border">
          <h3 className="font-semibold">Notifications</h3>
          {unread > 0 && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => markAllMutation.mutate()}
              disabled={markAllMutation.isPending}
            >
              <Check className="size-3 mr-1" /> Mark all
            </Button>
          )}
        </div>

        <div className="max-h-96 overflow-y-auto">
          {listQuery.isLoading && (
            <div className="p-4 space-y-2">
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
            </div>
          )}

          {listQuery.data && listQuery.data.items.length === 0 && (
            <p className="p-6 text-center text-sm text-muted-foreground">
              No notifications yet.
            </p>
          )}

          {listQuery.data && listQuery.data.items.length > 0 && (
            <ul>
              {listQuery.data.items.map((n) => (
                <NotificationItem
                  key={n.id}
                  notification={n}
                  onClose={() => setOpen(false)}
                />
              ))}
            </ul>
          )}
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}

// ---------------------------------------------------------------------------
// One row in the dropdown. Clicking marks it read AND navigates to the related
// entity. We do an optimistic flip so the row dims immediately.
// ---------------------------------------------------------------------------
function NotificationItem({
  notification: n,
  onClose,
}: {
  notification: NotificationDto
  onClose: () => void
}) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const markReadMutation = useMutation({
    mutationFn: () => notificationsApi.markAsRead(n.id),
    onMutate: async () => {
      // Optimistically flip this row in the cached list and decrement unread count.
      await queryClient.cancelQueries({ queryKey: ['notifications-list'] })
      const list = queryClient.getQueryData<{ items: NotificationDto[] }>(['notifications-list'])
      if (list) {
        queryClient.setQueryData(['notifications-list'], {
          ...list,
          items: list.items.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)),
        })
      }
      const prevCount = queryClient.getQueryData<number>(['notifications-unread']) ?? 0
      if (!n.isRead) {
        queryClient.setQueryData(['notifications-unread'], Math.max(0, prevCount - 1))
      }
    },
    // No rollback — read is idempotent, server failure is rare and harmless.
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications-unread'] })
    },
  })

  const handleClick = () => {
    if (!n.isRead) markReadMutation.mutate()
    const target = targetUrl(n)
    if (target) navigate(target)
    onClose()
  }

  return (
    <li>
      <button
        type="button"
        onClick={handleClick}
        className={`w-full text-left px-4 py-3 hover:bg-accent transition-colors flex gap-3 ${
          n.isRead ? 'opacity-60' : ''
        }`}
      >
        <div className="mt-0.5">{iconFor(n.type)}</div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium">{n.title}</p>
          <p className="text-xs text-muted-foreground line-clamp-2">{n.message}</p>
          <p className="text-xs text-muted-foreground mt-1">{relativeTime(n.createdAt)}</p>
        </div>
        {!n.isRead && (
          <span className="size-2 rounded-full bg-primary mt-2 shrink-0" aria-hidden />
        )}
      </button>
    </li>
  )
}

// ---------------------------------------------------------------------------
function iconFor(type: NotificationType) {
  const cls = 'size-4'
  switch (type) {
    case NotificationType.NewFollower:
      return <UserPlus className={`${cls} text-blue-500`} />
    case NotificationType.NewLike:
      return <Heart className={`${cls} text-pink-500`} />
    case NotificationType.NewComment:
      return <MessageCircle className={`${cls} text-amber-500`} />
    case NotificationType.AnalysisResolved:
      return <Target className={`${cls} text-green-500`} />
  }
}

function targetUrl(n: NotificationDto): string | null {
  switch (n.type) {
    case NotificationType.NewFollower:
      return n.actorUserId ? `/users/${n.actorUserId}` : null
    case NotificationType.NewLike:
    case NotificationType.NewComment:
    case NotificationType.AnalysisResolved:
      return n.relatedEntityId ? `/analyses/${n.relatedEntityId}` : null
  }
}

function relativeTime(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime()
  const mins = Math.floor(diff / 60_000)
  if (mins < 1) return 'just now'
  if (mins < 60) return `${mins}m ago`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.floor(hours / 24)
  if (days < 7) return `${days}d ago`
  return new Date(iso).toLocaleDateString()
}
