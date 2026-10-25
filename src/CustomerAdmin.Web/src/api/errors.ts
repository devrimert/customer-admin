import { t } from '../i18n'
import { ApiError } from './client'

/** Message for the user, based on the status code. */
export function errorMessage(error: unknown): string {
  if (!(error instanceof ApiError)) {
    return t.errors.network
  }

  switch (error.status) {
    case 400:
    case 422:
      return t.errors.invalid
    case 404:
      return t.errors.notFound
    case 409:
      return t.errors.conflict
    default:
      return t.errors.unexpected
  }
}
