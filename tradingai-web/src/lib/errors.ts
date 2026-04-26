import { AxiosError } from 'axios'

type ValidationFailure = { propertyName?: string; errorMessage?: string }
type ApiErrorPayload = {
  message?: string
  errors?: ValidationFailure[] | Record<string, string[]>
}

/**
 * Extracts a user-friendly error message from an axios error.
 * Handles ASP.NET-style validation payloads, plain message responses,
 * and falls back to a sensible default.
 */
export function getApiErrorMessage(err: unknown, fallback = 'Something went wrong'): string {
  if (err instanceof AxiosError) {
    const data = err.response?.data as ApiErrorPayload | undefined

    // FluentValidation array form: [{ propertyName, errorMessage }, ...]
    if (Array.isArray(data?.errors) && data!.errors.length > 0) {
      const messages = data!.errors
        .map((e) => e.errorMessage)
        .filter(Boolean)
        .join(', ')
      if (messages) return messages
    }

    // ASP.NET ProblemDetails form: { errors: { Field: ["msg1", "msg2"] } }
    if (data?.errors && !Array.isArray(data.errors)) {
      const messages = Object.values(data.errors).flat().filter(Boolean).join(', ')
      if (messages) return messages
    }

    if (data?.message) return data.message
    if (err.message) return err.message
  }
  return fallback
}
