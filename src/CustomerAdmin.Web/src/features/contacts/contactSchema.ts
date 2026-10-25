import { z } from 'zod'
import type { ContactRequest } from '../../api/types'
import { email, emptyToNull, phone, requiredText, text } from '../../validation/rules'

export const contactSchema = z.object({
  firstName: requiredText(100),
  lastName: requiredText(100),
  position: text(100),
  email,
  phone,
  note: text(2000),
  isPrimary: z.boolean(),
})

export type ContactFormValues = z.infer<typeof contactSchema>

export function toContactRequest(values: ContactFormValues): ContactRequest {
  return {
    firstName: values.firstName,
    lastName: values.lastName,
    position: emptyToNull(values.position),
    email: emptyToNull(values.email),
    phone: emptyToNull(values.phone),
    note: emptyToNull(values.note),
    isPrimary: values.isPrimary,
  }
}