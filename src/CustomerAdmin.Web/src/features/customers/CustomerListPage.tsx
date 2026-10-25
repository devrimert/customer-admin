import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { errorMessage } from '../../api/errors'
import type { CustomerSummary } from '../../api/types'
import SortableHeader from '../../components/SortableHeader'
import { useDebouncedValue } from '../../hooks/useDebouncedValue'
import { useSort } from '../../hooks/useSort'
import { formatDate, formatDateTime, t } from '../../i18n'
import { useCustomers } from './queries'

type SortKey = 'name' | 'city' | 'phone' | 'email' | 'contactCount' | 'status' | 'createdAt' | 'updatedAt'

const columns: { key: SortKey; label: string }[] = [
  { key: 'name', label: t.customers.fields.name },
  { key: 'city', label: t.customers.fields.city },
  { key: 'phone', label: t.customers.fields.phone },
  { key: 'email', label: t.customers.fields.email },
  { key: 'contactCount', label: t.customers.fields.contactCount },
  { key: 'status', label: t.customers.fields.status },
  { key: 'createdAt', label: t.common.created },
  { key: 'updatedAt', label: t.common.updated },
]

// Outside the component, so the sort hook gets the same function on every render
function sortValue(customer: CustomerSummary, key: SortKey) {
  switch (key) {
    case 'status':
      return customer.isActive ? 0 : 1
    case 'createdAt':
      return Date.parse(customer.createdAt)
    case 'updatedAt':
      return customer.updatedAt ? Date.parse(customer.updatedAt) : null
    default:
      return customer[key]
  }
}

export default function CustomerListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [includeInactive, setIncludeInactive] = useState(false)
  const debouncedSearch = useDebouncedValue(search, 300)

  const { data, isPending, error } = useCustomers(debouncedSearch, includeInactive)
  const { sorted: customers, sort, toggle } = useSort(data, sortValue, { key: 'name', direction: 'asc' })

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
        <div className="table-card">
          <table>
            <thead>
              <tr>
                {columns.map((column) => (
                  <SortableHeader key={column.key} label={column.label} sortKey={column.key} sort={sort} onSort={toggle} />
                ))}
              </tr>
            </thead>
            <tbody>
              {customers.map((customer) => (
                // The whole row opens the customer. The name stays a real link for keyboard and screen readers.
                <tr
                  key={customer.id}
                  className={customer.isActive ? 'clickable' : 'clickable inactive'}
                  onClick={() => navigate(`/customers/${customer.id}`)}
                >
                  <td>
                    <Link to={`/customers/${customer.id}`}>{customer.name}</Link>
                  </td>
                  <td>{customer.city}</td>
                  <td>{customer.phone}</td>
                  <td>{customer.email}</td>
                  <td>{customer.contactCount}</td>
                  <td>
                    <span className={customer.isActive ? 'badge active' : 'badge inactive'}>
                      {customer.isActive ? t.customers.active : t.customers.inactive}
                    </span>
                  </td>
                  <td title={formatDateTime(customer.createdAt)}>{formatDate(customer.createdAt)}</td>
                  <td title={customer.updatedAt ? formatDateTime(customer.updatedAt) : undefined}>
                    {customer.updatedAt ? formatDate(customer.updatedAt) : t.common.empty}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  )
}
