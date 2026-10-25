import type {
  Contact,
  ContactRequest,
  CustomerDetail,
  CustomerRequest,
  CustomerSummary,
  UpdateContactRequest,
  UpdateCustomerRequest,
} from './types'

/** Error body from the API (RFC 9457 problem details). */
export type ProblemDetails = {
  status?: number
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

/** Thrown for every response that is not 2xx, so pages can react to the status code. */
export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails | null

  constructor(status: number, problem: ProblemDetails | null) {
    super(problem?.title ?? `Request failed with status ${status}`)
    this.status = status
    this.problem = problem
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })

  if (!response.ok) {
    // Error bodies are problem details. If the body can't be read, we still have the status code.
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null
    throw new ApiError(response.status, problem)
  }

  // 204 No Content has no body
  return (response.status === 204 ? undefined : await response.json()) as T
}

/** JSON Patch the way the API wants it: test the version first, then one replace per changed field. */
function patchBody(version: number, changes: Record<string, unknown>) {
  return JSON.stringify([
    { op: 'test', path: '/version', value: version },
    ...Object.entries(changes).map(([field, value]) => ({ op: 'replace', path: `/${field}`, value })),
  ])
}

const jsonPatchHeaders = { 'Content-Type': 'application/json-patch+json' }

export const api = {
  getCustomers: (search: string, includeInactive: boolean) => {
    const query = new URLSearchParams({ search, includeInactive: String(includeInactive) })
    return request<CustomerSummary[]>(`/api/customers?${query}`)
  },

  getCustomer: (id: number) => request<CustomerDetail>(`/api/customers/${id}`),

  createCustomer: (body: CustomerRequest) =>
    request<CustomerDetail>('/api/customers', { method: 'POST', body: JSON.stringify(body) }),

  updateCustomer: (id: number, body: UpdateCustomerRequest) =>
    request<CustomerDetail>(`/api/customers/${id}`, { method: 'PUT', body: JSON.stringify(body) }),

  patchCustomer: (id: number, version: number, changes: Partial<UpdateCustomerRequest>) =>
    request<CustomerDetail>(`/api/customers/${id}`, {
      method: 'PATCH',
      headers: jsonPatchHeaders,
      body: patchBody(version, changes),
    }),

  deleteCustomer: (id: number) => request<void>(`/api/customers/${id}`, { method: 'DELETE' }),

  createContact: (customerId: number, body: ContactRequest) =>
    request<Contact>(`/api/customers/${customerId}/contacts`, {
      method: 'POST',
      body: JSON.stringify(body),
    }),

  updateContact: (customerId: number, contactId: number, body: UpdateContactRequest) =>
    request<Contact>(`/api/customers/${customerId}/contacts/${contactId}`, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),

  patchContact: (
    customerId: number,
    contactId: number,
    version: number,
    changes: Partial<UpdateContactRequest>,
  ) =>
    request<Contact>(`/api/customers/${customerId}/contacts/${contactId}`, {
      method: 'PATCH',
      headers: jsonPatchHeaders,
      body: patchBody(version, changes),
    }),

  deleteContact: (customerId: number, contactId: number) =>
    request<void>(`/api/customers/${customerId}/contacts/${contactId}`, { method: 'DELETE' }),
}