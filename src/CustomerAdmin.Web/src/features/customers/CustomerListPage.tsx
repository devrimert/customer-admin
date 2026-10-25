import { useState } from 'react'
import { Link } from 'react-router'
import { errorMessage } from '../../api/errors'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
import { t } from '../../i18n'
import { useCustomers } from './queries'

export default function CustomerListPage() {
  const [search, setSearch] = useState('')
  const [includeInactive, setIncludeInactive] = useState(false)
  const debouncedSearch = useDebouncedValue(search, 300)

  const { data: customers, isPending, error } = useCustomers(debouncedSearch, includeInactive)

  return (
    <>
      <h1>{t.customers.title}</h1>

      <div className="toolbar">
        <input
          type="search"
          placeholder={t.customers.searchPlaceholder}
          aria-label={t.customers.searchPlaceholder}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <label>
          <input
            type="checkbox"
            checked={includeInactive}
            onChange={(e) => setIncludeInactive(e.target.checked)}
          />
          {t.customers.showInactive}
        </label>
      </div>

      {isPending && <p aria-busy="true">{t.common.loading}</p>}
      {error && <p role="alert">{errorMessage(error)}</p>}
      {customers?.length === 0 && <p>{t.customers.empty}</p>}

      {customers && customers.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>{t.customers.fields.name}</th>
              <th>{t.customers.fields.city}</th>
              <th>{t.customers.fields.phone}</th>
              <th>{t.customers.fields.email}</th>
              <th>{t.customers.fields.contactCount}</th>
              <th>{t.customers.fields.status}</th>
            </tr>
          </thead>
          <tbody>
            {customers.map((customer) => (
              <tr key={customer.id} className={customer.isActive ? undefined : 'inactive'}>
                <td>
                  <Link to={`/customers/${customer.id}`}>{customer.name}</Link>
                </td>
                <td>{customer.city}</td>
                <td>{customer.phone}</td>
                <td>{customer.email}</td>
                <td>{customer.contactCount}</td>
                <td>{customer.isActive ? t.customers.active : t.customers.inactive}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  )
}