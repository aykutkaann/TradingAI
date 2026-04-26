import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuthStore } from '@/stores/authStore'

/**
 * Gate for routes that require an authenticated user.
 *
 * C# parallel: this is the React equivalent of [Authorize] on a controller.
 * Instead of returning 401, it redirects the user to /login and remembers
 * where they were trying to go (via location state) so we can send them
 * back after a successful sign-in.
 *
 * Usage in the router tree:
 *   <Route element={<ProtectedRoute />}>
 *     <Route path="/profile" element={<ProfilePage />} />
 *   </Route>
 *
 * The <Outlet /> renders whatever child route matched, but only if the
 * user is authenticated.
 */
export function ProtectedRoute() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return <Outlet />
}
