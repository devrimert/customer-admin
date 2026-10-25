import { z } from 'zod'
import { t } from '../i18n'

// Same limits as the API. Optional fields can be empty and are sent as null.
export const text = (max: number) => z.string().trim().max(max, t.validation.tooLong(max))

export const requiredText = (max: number) => text(max).min(1, t.validation.required)

export const email = z.union([
  z.literal(''),
  z.email(t.validation.email).max(254, t.validation.tooLong(254)),
])

// Like [Phone] in the API: digits, spaces and + - . ( ), at least 4 characters
export const phone = text(50).refine(
  (value) => value === '' || (value.length >= 4 && /^\+?[0-9\s().-]+$/.test(value)),
  t.validation.phone,
)

// Inputs give '' for an empty field, the API wants null
export const emptyToNull = (value: string) => value || null