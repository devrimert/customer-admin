import { useMutation, useQueryClient, type QueryClient } from '@tanstack/react-query'
import { api } from '../../api/client'
import type { Contact, ContactRequest, CustomerDetail, UpdateContactRequest } from '../../api/types'
import { customerKeys } from '../customers/queries'

// Contacts are shown as part of the customer, so after a change we reload the customer data
function useRefresh() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: customerKeys.all })
}

// Put the changed contact into the cached customer right away, so the next edit has its new version
function replaceContact(queryClient: QueryClient, customerId: number, contact: Contact) {
  queryClient.setQueryData<CustomerDetail>(customerKeys.detail(customerId), (customer) =>
    customer && { ...customer, contacts: customer.contacts.map((c) => (c.id === contact.id ? contact : c)) },
  )
}

export function useCreateContact(customerId: number) {
  const refresh = useRefresh()
  return useMutation({
    mutationFn: (body: ContactRequest) => api.createContact(customerId, body),
    onSettled: refresh,
  })
}

export function useUpdateContact(customerId: number) {
  const queryClient = useQueryClient()
  const refresh = useRefresh()
  return useMutation({
    mutationFn: ({ contactId, body }: { contactId: number; body: UpdateContactRequest }) =>
      api.updateContact(customerId, contactId, body),
    onSuccess: (contact) => replaceContact(queryClient, customerId, contact),
    onSettled: refresh,
  })
}

export function usePatchContact(customerId: number) {
  const queryClient = useQueryClient()
  const refresh = useRefresh()
  return useMutation({
    mutationFn: ({
      contactId,
      version,
      changes,
    }: {
      contactId: number
      version: number
      changes: Partial<UpdateContactRequest>
    }) => api.patchContact(customerId, contactId, version, changes),
    onSuccess: (contact) => replaceContact(queryClient, customerId, contact),
    onSettled: refresh,
  })
}

export function useDeleteContact(customerId: number) {
  const refresh = useRefresh()
  return useMutation({
    mutationFn: (contactId: number) => api.deleteContact(customerId, contactId),
    onSettled: refresh,
  })
}