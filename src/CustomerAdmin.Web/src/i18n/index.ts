import { de } from './de'

// The app has one language for now. To add another one, pick the texts here (or with react-i18next).
export const t = de

// Used for dates and numbers, e.g. 25.10.2026
export const locale = 'de-DE'

const dateTimeFormat = new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' })
const dateFormat = new Intl.DateTimeFormat(locale, { dateStyle: 'medium' })

/** Formats an ISO date from the API for the user, e.g. "25.10.2026, 19:00". */
export function formatDateTime(value: string): string {
  return dateTimeFormat.format(new Date(value))
}

/** Date only, for tables, e.g. "25.10.2026". */
export function formatDate(value: string): string {
  return dateFormat.format(new Date(value))
}
