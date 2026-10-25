import { NavLink, Outlet } from 'react-router'
import { t } from '../i18n'

/** App frame: fixed sidebar on the left, the current page on the right. */
export default function Layout() {
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
      </aside>

      <main className="content">
        <Outlet />
      </main>
    </div>
  )
}