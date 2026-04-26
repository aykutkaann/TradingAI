import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'

import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { analysisApi } from '@/api/analysis'
import { AnalysisListCard } from '@/components/analysis/AnalysisListCard'
import { useAuthStore } from '@/stores/authStore'
import type { AnalysisDto, PagedResult } from '@/types/analysis'

const PAGE_SIZE = 12

export function FeedPage() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Feed</h1>
        <p className="text-muted-foreground">
          Latest analyses published by the community
        </p>
      </div>

      <Tabs defaultValue="latest" className="w-full">
        <TabsList>
          <TabsTrigger value="latest">Latest</TabsTrigger>
          <TabsTrigger value="following" disabled={!isAuthenticated}>
            Following
          </TabsTrigger>
        </TabsList>

        <TabsContent value="latest" className="mt-6">
          <FeedList
            queryKey={['public-feed']}
            fetcher={(page) => analysisApi.getPublicFeed(page, PAGE_SIZE)}
            emptyText="No published analyses yet. Be the first!"
          />
        </TabsContent>

        <TabsContent value="following" className="mt-6">
          {isAuthenticated ? (
            <FeedList
              queryKey={['following-feed']}
              fetcher={(page) => analysisApi.getFollowingFeed(page, PAGE_SIZE)}
              emptyText="You're not following anyone yet, or they haven't published anything."
            />
          ) : (
            <div className="text-center py-16 border border-dashed border-border rounded-lg">
              <p className="text-muted-foreground mb-4">
                Sign in to see analyses from people you follow.
              </p>
              <Button asChild>
                <Link to="/login">Sign in</Link>
              </Button>
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  )
}

// ---------------------------------------------------------------------------
// Reusable paged feed list. The two tabs differ only in queryKey + fetcher,
// so we extract the rendering / pagination here. Each tab keeps its own
// `page` state so switching tabs doesn't reset the other one.
// ---------------------------------------------------------------------------
function FeedList({
  queryKey,
  fetcher,
  emptyText,
}: {
  queryKey: readonly unknown[]
  fetcher: (page: number) => Promise<PagedResult<AnalysisDto>>
  emptyText: string
}) {
  const [page, setPage] = useState(1)

  const { data, isLoading, isError } = useQuery({
    queryKey: [...queryKey, page],
    queryFn: () => fetcher(page),
  })

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {Array.from({ length: 6 }).map((_, i) => (
          <Skeleton key={i} className="h-32 w-full" />
        ))}
      </div>
    )
  }

  if (isError) {
    return <p className="text-destructive">Failed to load. Try refreshing.</p>
  }

  if (!data || data.items.length === 0) {
    return (
      <div className="text-center py-16 border border-dashed border-border rounded-lg">
        <p className="text-muted-foreground">{emptyText}</p>
      </div>
    )
  }

  return (
    <>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {data.items.map((a) => (
          <AnalysisListCard key={a.id} analysis={a} />
        ))}
      </div>

      <div className="flex items-center justify-between pt-4">
        <Button
          variant="outline"
          size="sm"
          disabled={!data.hasPrev}
          onClick={() => setPage((p) => Math.max(1, p - 1))}
        >
          Previous
        </Button>
        <span className="text-sm text-muted-foreground">
          Page {data.page} of {data.totalPages || 1}
        </span>
        <Button
          variant="outline"
          size="sm"
          disabled={!data.hasNext}
          onClick={() => setPage((p) => p + 1)}
        >
          Next
        </Button>
      </div>
    </>
  )
}
