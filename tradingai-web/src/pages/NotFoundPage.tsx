import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/button'

export function NotFoundPage() {
  return (
    <div className="max-w-md mx-auto text-center py-24 space-y-4">
      <h1 className="text-7xl font-bold text-primary">404</h1>
      <h2 className="text-2xl font-semibold">Page not found</h2>
      <p className="text-muted-foreground">
        The page you're looking for doesn't exist or has been moved.
      </p>
      <div className="flex gap-2 justify-center pt-4">
        <Button asChild>
          <Link to="/">Go home</Link>
        </Button>
        <Button asChild variant="outline">
          <Link to="/feed">Browse feed</Link>
        </Button>
      </div>
    </div>
  )
}
