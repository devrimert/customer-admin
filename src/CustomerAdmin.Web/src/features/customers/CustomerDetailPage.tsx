import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { errorMessage } from '../../api/errors'
import type { UpdateCustomerRequest } from '../../api/types'
import InlineEdit from '../../components/InlineEdit'
import { formatDateTime, t } from '../../i18n'
import ContactsTable from '../contacts/ContactsTable'
import CustomerFormDialog from './CustomerFormDialog'
import { customerSchema } from './customerSchema'
import { useCustomer, useDeleteCustomer, usePatchCustomer } from './queries'

type EditableKey = 'email' | 'phone' | 'street' | 'postalCode' | 'city' | 'note'

const fields: { key: EditableKey; type?: 'email' | 'tel'; multiline?: boolean }[] = [
  { key: 'email', type: 'email' },
  { key: 'phone', type: 'tel' },
  { key: 'street' },
  { key: 'postalCode' },
  { key: 'city' },
  { key: 'note', multiline: true },
]

export default function CustomerDetailPage() {
  const id = Number(useParams().id)
  const navigate = useNavigate()
  const [editing, setEditing] = useState(false)

  // All hooks before the first return, React needs the same order on every render
  const { data: customer, isPending, error } = useCustomer(id)
  const patchField = usePatchCustomer(id)
  const patchStatus = usePatchCustomer(id)
  const remove = useDeleteCustomer(id)

  if (isPending) {
    return <p aria-busy="true">{t.common.loading}</p>
  }

  if (error) {
    return <p role="alert">{errorMessage(error)}</p>
  }

  const saveField = (key: EditableKey, value: string | null) => {
    const changes: Partial<UpdateCustomerRequest> = {}
    changes[key] = value
    return patchField.mutateAsync({ version: customer.version, changes })
  }

  const toggleActive = () =>
    patchStatus.mutate({ version: customer.version, changes: { isActive: !customer.isActive } })

  const deleteCustomer = () => {
    if (!window.confirm(t.customers.confirmDelete(customer.name))) return
    remove.mutate(undefined, { onSuccess: () => navigate('/customers') })
  }

  const actionError = patchStatus.error ?? remove.error

  return (
    <>
      <nav aria-label="breadcrumb">
        <ul>
          <li>
            <Link to="/customers">{t.customers.title}</Link>
          </li>
          <li>{customer.name}</li>
        </ul>
      </nav>

      <div className="page-header">
        <h1>
          {customer.name} {!customer.isActive && <mark>{t.customers.inactive}</mark>}
        </h1>
        <div className="actions">
          <button type="button" className="secondary outline" onClick={() => setEditing(true)}>
            {t.common.edit}
          </button>
          <button
            type="button"
            className="secondary outline"
            onClick={toggleActive}
            aria-busy={patchStatus.isPending}
            disabled={patchStatus.isPending}
          >
            {customer.isActive ? t.customers.deactivate : t.customers.activate}
          </button>
          <button type="button" className="contrast outline" onClick={deleteCustomer} disabled={remove.isPending}>
            {t.common.delete}
          </button>
        </div>
      </div>

      {actionError && <p role="alert">{errorMessage(actionError)}</p>}

      <article>
        <dl>
          {fields.map(({ key, type, multiline }) => (
            <div key={key} className="field">
              <dt>{t.customers.fields[key]}</dt>
              <dd>
                <InlineEdit
                  label={t.customers.fields[key]}
                  value={customer[key]}
                  schema={customerSchema.shape[key]}
                  type={type}
                  multiline={multiline}
                  onSave={(value) => saveField(key, value)}
                />
              </dd>
            </div>
          ))}
        </dl>
        <footer>
          <small>
            {t.customers.fields.createdAt}: {formatDateTime(customer.createdAt)}
            {customer.updatedAt && (
              <> · {t.customers.fields.updatedAt}: {formatDateTime(customer.updatedAt)}</>
            )}
          </small>
        </footer>
      </article>

      <ContactsTable customer={customer} />

      {editing && <CustomerFormDialog customer={customer} onClose={() => setEditing(false)} />}
    </>
  )
}