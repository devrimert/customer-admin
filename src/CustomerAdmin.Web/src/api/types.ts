import type { components } from './schema'

// Short names for the generated types
type Schemas = components['schemas']

export type CustomerSummary = Schemas['CustomerSummaryResponse']
export type CustomerDetail = Schemas['CustomerDetailResponse']
export type CustomerRequest = Schemas['CustomerRequest']
export type UpdateCustomerRequest = Schemas['UpdateCustomerRequest']
export type Contact = Schemas['ContactResponse']
export type ContactRequest = Schemas['ContactRequest']
export type UpdateContactRequest = Schemas['UpdateContactRequest']
