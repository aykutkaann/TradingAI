export const NotificationType = {
  NewFollower: 0,
  NewLike: 1,
  NewComment: 2,
  AnalysisResolved: 3,
} as const
export type NotificationType = (typeof NotificationType)[keyof typeof NotificationType]

export type NotificationDto = {
  id: string
  type: NotificationType
  title: string
  message: string
  relatedEntityId: string | null
  actorUserId: string | null
  isRead: boolean
  createdAt: string
  readAt: string | null
}
