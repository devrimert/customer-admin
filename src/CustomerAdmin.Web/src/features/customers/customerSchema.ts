import { z } from 'zod'
import type { CustomerRequest } from '../../api/types'
import { email, emptyToNull, phone, requiredText, text } from '../../validation/rules'

export const customerSchema = z.object({
  name: requiredText(200),
  email,
  phone,
  street: text(200),
  postalCode: text(20),
  city: text(100),
  note: text(2000),
})

export type CustomerFormValues = z.infer<typeof customerSchema>

export function toCustomerRequest(values: CustomerFormValues): CustomerRequest {
  return {
    name: values.name,
    email: emptyToNull(values.email),
    phone: emptyToNull(values.phone),
    street: emptyToNull(values.street),
    postalCode: emptyToNull(values.postalCode),
    city: emptyToNull(values.city),
    note: emptyToNull(values.note),
  }
}