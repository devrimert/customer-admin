import { useState } from 'react'
import { errorMessage } from '../../api/errors'
import type { Contact, CustomerDetail, UpdateContactRequest } from '../../api/types'
import InlineEdit from '../../components/InlineEdit'
import { t } from '../../i18n'
import ContactFormDialog from './ContactFormDialog'
import { contactSchema } from './contactSchema'
import { useDeleteContact, usePatchContact } from './queries'

type EditableKey = 'position' | 'phone' | 'email'

const columns: { key: EditableKey; type?: 'tel' | 'email' }[] = [
  { key: 'position' },
  { key: 'phone', type: 'tel' },
  { key: 'email', type: 'email' },
]

export default function ContactsTable({ customer }: { customer: CustomerDetail }) {
  // Which dialog is open: a contact to edit, 'new' for a new one, or none
  const [dialog, setDialog] = useState<Contact | 'new' | null>(null)

  // Separate mutations, so an error in a cell doesn't show up at the top of the table as well
  const patchField = usePatchContact(customer.id)
  const patchPrimary = usePatchContact(customer.id)
  const remove = useDeleteContact(customer.id)

  const saveField = (contact: Contact, key: EditableKey, value: string | null) => {
    const changes: Partial<UpdateContactRequest> = {}
    changes[key] = value
    return patchField.mutateAsync({ contactId: contact.id, version: contact.version, changes })
  }

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
        <button type="button" className="outline" onClick={() => setDialog('new')}>
          {t.contacts.new}
        </button>
      </div>

      {error && <p role="alert">{errorMessage(error)}</p>}

      {customer.contacts.length === 0 ? (
        <p>{t.contacts.empty}</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>
                <span className="visually-hidden">{t.contacts.primary}</span>
              </th>
              <th>{t.contacts.fields.name}</th>
              <th>{t.contacts.fields.position}</th>
              <th>{t.contacts.fields.phone}</th>
              <th>{t.contacts.fields.email}</th>
              <th>
                <span className="visually-hidden">{t.common.edit}</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {customer.contacts.map((contact) => (
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
                <td>
                  {contact.firstName} {contact.lastName}
                </td>
                {columns.map(({ key, type }) => (
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
                <td className="row-actions">
                  <button type="button" className="link" onClick={() => setDialog(contact)}>
                    {t.common.edit}
                  </button>
                  <button type="button" className="link danger" onClick={() => deleteContact(contact)}>
                    {t.common.delete}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
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