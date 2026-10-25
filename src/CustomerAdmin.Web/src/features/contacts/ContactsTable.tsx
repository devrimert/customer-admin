import { useState } from 'react'
import { errorMessage } from '../../api/errors'
import type { Contact, CustomerDetail, UpdateContactRequest } from '../../api/types'
import { EditIcon, PlusIcon, TrashIcon } from '../../components/Icons'
import InlineEdit from '../../components/InlineEdit'
import SortableHeader from '../../components/SortableHeader'
import { useSort } from '../../hooks/useSort'
import { formatDate, formatDateTime, t } from '../../i18n'
import ContactFormDialog from './ContactFormDialog'
import ContactNameEdit from './ContactNameEdit'
import { contactSchema } from './contactSchema'
import { useDeleteContact, usePatchContact } from './queries'

type EditableKey = 'position' | 'phone' | 'email'
type SortKey = 'name' | 'position' | 'createdAt' | 'updatedAt'

const editableColumns: { key: EditableKey; type?: 'tel' | 'email' }[] = [
  { key: 'position' },
  { key: 'phone', type: 'tel' },
  { key: 'email', type: 'email' },
]

// Outside the component, so the sort hook gets the same functions on every render
function sortValue(contact: Contact, key: SortKey) {
  switch (key) {
    case 'name':
      return `${contact.lastName} ${contact.firstName}`
    case 'position':
      return contact.position
    case 'createdAt':
      return Date.parse(contact.createdAt)
    case 'updatedAt':
      return contact.updatedAt ? Date.parse(contact.updatedAt) : null
  }
}

// The primary contact stays on top, whatever the sort is
const isPrimary = (contact: Contact) => contact.isPrimary

export default function ContactsTable({ customer }: { customer: CustomerDetail }) {
  // Which dialog is open: a contact to edit, 'new' for a new one, or none
  const [dialog, setDialog] = useState<Contact | 'new' | null>(null)

  // Separate mutations, so an error in a cell doesn't show up at the top of the table as well
  const patchField = usePatchContact(customer.id)
  const patchPrimary = usePatchContact(customer.id)
  const remove = useDeleteContact(customer.id)

  const { sorted: contacts = [], sort, toggle } = useSort(
    customer.contacts,
    sortValue,
    { key: 'name', direction: 'asc' },
    isPrimary,
  )

  const patch = (contact: Contact, changes: Partial<UpdateContactRequest>) =>
    patchField.mutateAsync({ contactId: contact.id, version: contact.version, changes })

  const saveField = (contact: Contact, key: EditableKey, value: string | null) => patch(contact, { [key]: value })

  // First and last name go in one request, so we don't need two versions
  const saveName = (contact: Contact, firstName: string, lastName: string) =>
    patch(contact, { firstName, lastName })

  const makePrimary = (contact: Contact) =>
    patchPrimary.mutate({ contactId: contact.id, version: contact.version, changes: { isPrimary: true } })

  const deleteContact = (contact: Contact) => {
    if (window.confirm(t.contacts.confirmDelete(`${contact.firstName} ${contact.lastName}`))) {
      remove.mutate(contact.id)
    }
  }

  const error = patchPrimary.error ?? remove.error

  return (
    <section>
      <div className="page-header">
        <h2>{t.contacts.title}</h2>
        <button type="button" className="outline with-icon" onClick={() => setDialog('new')}>
          <PlusIcon /> {t.contacts.new}
        </button>
      </div>

      {error && <p role="alert">{errorMessage(error)}</p>}

      {contacts.length === 0 ? (
        <p>{t.contacts.empty}</p>
      ) : (
        <div className="table-card">
          <table>
            <thead>
              <tr>
                <th>
                  <span className="visually-hidden">{t.contacts.primary}</span>
                </th>
                <SortableHeader label={t.contacts.fields.name} sortKey="name" sort={sort} onSort={toggle} />
                <SortableHeader label={t.contacts.fields.position} sortKey="position" sort={sort} onSort={toggle} />
                <th>{t.contacts.fields.phone}</th>
                <th>{t.contacts.fields.email}</th>
                <SortableHeader label={t.common.created} sortKey="createdAt" sort={sort} onSort={toggle} />
                <SortableHeader label={t.common.updated} sortKey="updatedAt" sort={sort} onSort={toggle} />
                <th>
                  <span className="visually-hidden">{t.common.edit}</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {contacts.map((contact) => (
                <tr key={contact.id}>
                  <td>
                    <button
                      type="button"
                      className="icon star"
                      title={contact.isPrimary ? t.contacts.primary : t.contacts.makePrimary}
                      aria-label={contact.isPrimary ? t.contacts.primary : t.contacts.makePrimary}
                      aria-pressed={contact.isPrimary}
                      disabled={contact.isPrimary || patchPrimary.isPending}
                      onClick={() => makePrimary(contact)}
                    >
                      {contact.isPrimary ? '★' : '☆'}
                    </button>
                  </td>
                  <td className="name-cell">
                    <ContactNameEdit
                      contact={contact}
                      onSave={(firstName, lastName) => saveName(contact, firstName, lastName)}
                    />
                  </td>
                  {editableColumns.map(({ key, type }) => (
                    <td key={key}>
                      <InlineEdit
                        label={t.contacts.fields[key]}
                        value={contact[key]}
                        schema={contactSchema.shape[key]}
                        type={type}
                        onSave={(value) => saveField(contact, key, value)}
                      />
                    </td>
                  ))}
                  <td title={formatDateTime(contact.createdAt)}>{formatDate(contact.createdAt)}</td>
                  <td title={contact.updatedAt ? formatDateTime(contact.updatedAt) : undefined}>
                    {contact.updatedAt ? formatDate(contact.updatedAt) : t.common.empty}
                  </td>
                  <td className="row-actions">
                    <button
                      type="button"
                      className="icon"
                      title={t.common.edit}
                      aria-label={t.common.edit}
                      onClick={() => setDialog(contact)}
                    >
                      <EditIcon />
                    </button>
                    <button
                      type="button"
                      className="icon danger"
                      title={t.common.delete}
                      aria-label={t.common.delete}
                      onClick={() => deleteContact(contact)}
                    >
                      <TrashIcon />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {dialog && (
        <ContactFormDialog
          customerId={customer.id}
          contact={dialog === 'new' ? undefined : dialog}
          onClose={() => setDialog(null)}
        />
      )}
    </section>
  )
}
