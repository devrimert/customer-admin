import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../../api/client'
import type { CustomerRequest, UpdateCustomerRequest } from '../../api/types'

/**
 * Cache keys for customer data. Everything starts with 'customers',
 * so after a change we can refresh all customer data with one call.
 */
export const customerKeys = {
  all: ['customers'] as const,
  lists: ['customers', 'list'] as const,
  list: (search: string, includeInactive: boolean) =>
    ['customers', 'list', { search, includeInactive }] as const,
  detail: (id: number) => ['customers', 'detail', id] as const,
}

export function useCustomers(search: string, includeInactive: boolean) {
  return useQuery({
    queryKey: customerKeys.list(search, includeInactive),
    queryFn: () => api.getCustomers(search, includeInactive),
    // Keep showing the old list while the new search loads, so the table doesn't flicker
    placeholderData: keepPreviousData,
  })
}

export function useCustomer(id: number) {
  return useQuery({
    queryKey: customerKeys.detail(id),
    queryFn: () => api.getCustomer(id),
  })
}

/** After any change (or a failed one, e.g. 409), reload customer data so the page shows the current state. */
function useRefreshCustomers() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: customerKeys.all })
}

export function useCreateCustomer() {
  const refresh = useRefreshCustomers()
  return useMutation({
    mutationFn: (body: CustomerRequest) => api.createCustomer(body),
    onSettled: refresh,
  })
}

export function useUpdateCustomer(id: number) {
  const refresh = useRefreshCustomers()
  return useMutation({
    mutationFn: (body: UpdateCustomerRequest) => api.updateCustomer(id, body),
    onSettled: refresh,
  })
}

export function usePatchCustomer(id: number) {
  const refresh = useRefreshCustomers()
  return useMutation({
    mutationFn: ({ version, changes }: { version: number; changes: Partial<UpdateCustomerRequest> }) =>
      api.patchCustomer(id, version, changes),
    onSettled: refresh,
  })
}

export function useDeleteCustomer(id: number) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => api.deleteCustomer(id),
    onSuccess: () => {
      // The customer is gone: drop its cached detail instead of reloading it (that would be a 404)
      queryClient.removeQueries({ queryKey: customerKeys.detail(id) })
      return queryClient.invalidateQueries({ queryKey: customerKeys.lists })
    },
  })
}