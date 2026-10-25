import { Link } from 'react-router'
import { t } from '../i18n'

export default function NotFoundPage() {
  return (
    <>
      <h1>{t.notFound.title}</h1>
      <Link to="/customers">{t.notFound.backToList}</Link>
    </>
  )
}