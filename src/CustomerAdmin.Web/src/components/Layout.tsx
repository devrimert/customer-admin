import { useState } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router'
import CustomerFormDialog from '../features/customers/CustomerFormDialog'
import { t } from '../i18n'
import { GitHubIcon, LinkedInIcon, MailIcon, PlusIcon } from './Icons'

/** App frame: portfolio bar on top, fixed sidebar on the left, the current page on the right. */
export default function Layout() {
  const [creating, setCreating] = useState(false)
  const navigate = useNavigate()

  return (
    <>
      <header className="portfolio-bar">
        <span>{t.portfolio.note}</span>
        <nav aria-label={t.portfolio.links}>
          <a href="https://github.com/devrimert" target="_blank" rel="noreferrer">
            <GitHubIcon size={14} /> {t.portfolio.github}
          </a>
          <a href="https://www.linkedin.com/in/dmyoyen" target="_blank" rel="noreferrer">
            <LinkedInIcon size={14} /> {t.portfolio.linkedin}
          </a>
          <a href="mailto:devrim.yoyen@icloud.com">
            <MailIcon size={14} /> {t.portfolio.mail}
          </a>
        </nav>
      </header>

      <div className="layout">
        <aside className="sidebar">
          <Link to="/customers" className="brand">
            <span className="brand-mark" aria-hidden="true">
              {t.app.initials}
            </span>
            {t.app.title}
          </Link>

          <nav>
            <ul>
              <li>
                <NavLink to="/customers">{t.nav.customers}</NavLink>
              </li>
            </ul>
          </nav>

          <button type="button" className="new-customer" onClick={() => setCreating(true)}>
            <PlusIcon /> {t.nav.newCustomer}
          </button>
        </aside>

        <main className="content">
          <Outlet />
        </main>
      </div>

      {creating && (
        <CustomerFormDialog
          onClose={() => setCreating(false)}
          onSaved={(customer) => navigate(`/customers/${customer.id}`)}
        />
      )}
    </>
  )
}
