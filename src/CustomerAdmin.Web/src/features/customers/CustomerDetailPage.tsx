import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { errorMessage } from '../../api/errors'
import { formatDateTime, t } from '../../i18n'
import CustomerFormDialog from './CustomerFormDialog'
import { useCustomer, useDeleteCustomer, usePatchCustomer } from './queries'

export default function CustomerDetailPage() {
  const id = Number(useParams().id)
  const navigate = useNavigate()
  const [editing, setEditing] = useState(false)

  // All hooks come first: React requires the same hooks in the same order on every render
  const { data: customer, isPending, error } = useCustomer(id)
  const patch = usePatchCustomer(id)
  const remove = useDeleteCustomer(id)

  if (isPending) {
    return <p aria-busy="true">{t.common.loading}</p>
  }

  if (error) {
    return <p role="alert">{errorMessage(error)}</p>
  }

  const toggleActive = () =>
    patch.mutate({ version: customer.version, changes: { isActive: !customer.isActive } })

  const deleteCustomer = () => {
    if (!window.confirm(t.customers.confirmDelete(customer.name))) return
    remove.mutate(undefined, { onSuccess: () => navigate('/customers') })
  }

  const actionError = patch.error ?? remove.error

  const fields = [
    [t.customers.fields.email, customer.email],
    [t.customers.fields.phone, customer.phone],
    [t.customers.fields.street, customer.street],
    [t.customers.fields.postalCode, customer.postalCode],
    [t.customers.fields.city, customer.city],
    [t.customers.fields.note, customer.note],
  ] as const

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
            aria-busy={patch.isPending}
            disabled={patch.isPending}
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
          {fields.map(([label, value]) => (
            <div key={label} className="field">
              <dt>{label}</dt>
              <dd>{value ?? t.common.empty}</dd>
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

      <h2>{t.contacts.title}</h2>

      {customer.contacts.length === 0 ? (
        <p>{t.contacts.empty}</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>{t.contacts.fields.name}</th>
              <th>{t.contacts.fields.position}</th>
              <th>{t.contacts.fields.phone}</th>
              <th>{t.contacts.fields.email}</th>
            </tr>
          </thead>
          <tbody>
            {customer.contacts.map((contact) => (
              <tr key={contact.id}>
                <td>
                  {contact.isPrimary && <span title={t.contacts.primary}>★ </span>}
                  {contact.firstName} {contact.lastName}
                </td>
                <td>{contact.position}</td>
                <td>{contact.phone}</td>
                <td>{contact.email}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {editing && <CustomerFormDialog customer={customer} onClose={() => setEditing(false)} />}
    </>
  )
}