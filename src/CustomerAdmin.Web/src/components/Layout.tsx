import { useState } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router'
import CustomerFormDialog from '../features/customers/CustomerFormDialog'
import { t } from '../i18n'

/** App frame: fixed sidebar on the left, the current page on the right. */
export default function Layout() {
  const [creating, setCreating] = useState(false)
  const navigate = useNavigate()

  return (
    <div className="layout">
      <aside className="sidebar">
        <strong>{t.app.title}</strong>
        <nav>
          <ul>
            <li>
              <NavLink to="/customers">{t.nav.customers}</NavLink>
            </li>
          </ul>
        </nav>
        <button type="button" onClick={() => setCreating(true)}>
          {t.nav.newCustomer}
        </button>
      </aside>

      <main className="content">
        <Outlet />
      </main>

      {creating && (
        <CustomerFormDialog
          onClose={() => setCreating(false)}
          onSaved={(customer) => navigate(`/customers/${customer.id}`)}
        />
      )}
    </div>
  )
}