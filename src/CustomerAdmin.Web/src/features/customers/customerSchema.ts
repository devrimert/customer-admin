import { z } from 'zod'
import { t } from '../../i18n'
import type { CustomerRequest } from '../../api/types'

const text = (max: number) => z.string().trim().max(max, t.validation.tooLong(max))

// Same rules as the API. Empty optional fields are allowed and sent as null.
export const customerSchema = z.object({
  name: text(200).min(1, t.validation.required),
  email: z.union([z.literal(''), z.email(t.validation.email).max(254, t.validation.tooLong(254))]),
  phone: text(50).refine(
    (value) => value === '' || (value.length >= 4 && /^\+?[0-9\s().-]+$/.test(value)),
    t.validation.phone,
  ),
  street: text(200),
  postalCode: text(20),
  city: text(100),
  note: text(2000),
})

export type CustomerFormValues = z.infer<typeof customerSchema>

/** Form inputs are always strings. The API wants null for empty optional fields. */
export function toCustomerRequest(values: CustomerFormValues): CustomerRequest {
  return {
    name: values.name,
    email: values.email || null,
    phone: values.phone || null,
    street: values.street || null,
    postalCode: values.postalCode || null,
    city: values.city || null,
    note: values.note || null,
  }
}