import { useNavigate } from 'react-router-dom'
import { AxiosError } from 'axios'
import { toast } from 'sonner'
import { getApiErrorMessage } from '@/lib/errors'

/**
 * Hook returning a toast-error helper that surfaces rate-limit (429) responses
 * with an "Upgrade" action button. For all other errors it falls back to the
 * normal getApiErrorMessage path.
 *
 * Backend payload for 429 (see ExceptionHandlingMiddleware.cs):
 *   { status: 429, type: "rate-limit-exceeded", message, limitName, used, limit }
 *
 * Options:
 *   redirectOn429   — auto-navigate to /plans (with a toast) instead of showing
 *                     the inline upgrade button. Used by the analyze page so a
 *                     free user who's hit their 3-analysis lifetime cap gets
 *                     bounced straight to upgrade flow.
 */
type Opts = {
  redirectOn429?: boolean
}

export function useApiErrorToast(opts: Opts = {}) {
  const navigate = useNavigate()

  return (err: unknown, fallback = 'Something went wrong') => {
    if (err instanceof AxiosError && err.response?.status === 429) {
      const data = err.response.data as {
        type?: string
        message?: string
        limit?: number
        used?: number
      }
      const message =
        data?.message ?? "You've used your free analyses. Upgrade to keep going."

      if (opts.redirectOn429) {
        toast.info(message, {
          description: 'Redirecting you to plans…',
          duration: 4000,
        })
        navigate('/plans')
        return
      }

      toast.error(message, {
        description:
          data?.used !== undefined && data?.limit !== undefined
            ? `Used ${data.used} of ${data.limit}.`
            : undefined,
        action: {
          label: 'Upgrade',
          onClick: () => navigate('/plans'),
        },
        duration: 8000,
      })
      return
    }

    toast.error(getApiErrorMessage(err, fallback))
  }
}
