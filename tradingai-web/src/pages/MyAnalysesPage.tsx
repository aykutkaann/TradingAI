import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Plus } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { analysisApi } from '@/api/analysis'
import { AnalysisListCard } from '@/components/analysis/AnalysisListCard'

const PAGE_SIZE = 12

export function MyAnalysesPage() {
  const [page, setPage] = useState(1)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['my-analyses', page],
    queryFn: () => analysisApi.getMine(page, PAGE_SIZE),
  })

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-2 flex-wrap">
        <div>
          <h1 className="text-3xl font-bold">My Analyses</h1>
          <p className="text-muted-foreground">All analyses you've run</p>
        </div>
        <Button asChild>
          <Link to="/analyze">
            <Plus className="size-4 mr-1" /> New analysis
          </Link>
        </Button>
      </div>

      {isLoading && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-32 w-full" />
          ))}
        </div>
      )}

      {isError && (
        <p className="text-destructive">Failed to load analyses. Try refreshing.</p>
      )}

      {data && data.items.length === 0 && (
        <div className="text-center py-16 border border-dashed border-border rounded-lg">
          <p className="text-muted-foreground mb-4">
            You haven't run any analyses yet.
          </p>
          <Button asChild>
            <Link to="/analyze">Run your first analysis</Link>
          </Button>
        </div>
      )}

      {data && data.items.length > 0 && (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {data.items.map((a) => (
              <AnalysisListCard key={a.id} analysis={a} />
            ))}
          </div>

          {/* Pagination */}
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
      )}
    </div>
  )
}
