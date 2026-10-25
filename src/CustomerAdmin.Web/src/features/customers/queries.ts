import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { api } from '../../api/client'

/**
 * Cache keys for customer data. Everything starts with 'customers',
 * so after a change we can refresh all customer data with one call.
 */
export const customerKeys = {
  all: ['customers'] as const,
  list: (search: string, includeInactive: boolean) =>
    ['customers', 'list', { search, includeInactive }] as const,
  detail: (id: number) => ['customers', 'detail', id] as const,
}

export function useCustomers(search: string, includeInactive: boolean) {
  return useQuery({
    queryKey: customerKeys.list(search, includeInactive),
    queryFn: () => api.getCustomers(search, includeInactive),
    // Keep showing the old list while the new search loads
    placeholderData: keepPreviousData,
  })
}

export function useCustomer(id: number) {
  return useQuery({
    queryKey: customerKeys.detail(id),
    queryFn: () => api.getCustomer(id),
  })
}