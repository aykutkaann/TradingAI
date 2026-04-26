export type CommentDto = {
  id: string
  analysisId: string
  userId: string
  userDisplayName: string
  userAvatarUrl: string | null
  content: string
  createdAt: string
}
