import { useState, type KeyboardEvent } from 'react'
import { errorMessage } from '../../api/errors'
import type { Contact } from '../../api/types'
import { PencilIcon } from '../../components/Icons'
import { t } from '../../i18n'
import { contactSchema } from './contactSchema'

const nameSchema = contactSchema.pick({ firstName: true, lastName: true })

type ContactNameEditProps = {
  contact: Contact
  onSave: (firstName: string, lastName: string) => Promise<unknown>
}

/** The full name with one pencil. Opens two fields and saves both in one request. */
export default function ContactNameEdit({ contact, onSave }: ContactNameEditProps) {
  const [editing, setEditing] = useState(false)
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const start = () => {
    setFirstName(contact.firstName)
    setLastName(contact.lastName)
    setError(null)
    setEditing(true)
  }

  const save = async () => {
    const result = nameSchema.safeParse({ firstName, lastName })
    if (!result.success) {
      setError(result.error.issues[0].message)
      return
    }

    setSaving(true)
    try {
      await onSave(result.data.firstName, result.data.lastName)
      setEditing(false)
    } catch (e) {
      setError(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault()
      void save()
    } else if (e.key === 'Escape') {
      setEditing(false)
    }
  }

  if (!editing) {
    return (
      <span className="inline-edit">
        <span className="value">
          {contact.firstName} {contact.lastName}
        </span>
        <button
          type="button"
          className="icon edit-trigger"
          title={t.common.edit}
          aria-label={`${t.contacts.fields.name}: ${t.common.edit}`}
          onClick={start}
        >
          <PencilIcon size={14} />
        </button>
      </span>
    )
  }

  return (
    <span className="inline-edit editing name-edit">
      <input
        value={firstName}
        placeholder={t.contacts.fields.firstName}
        aria-label={t.contacts.fields.firstName}
        disabled={saving}
        autoFocus
        onKeyDown={onKeyDown}
        onChange={(e) => setFirstName(e.target.value)}
      />
      <input
        value={lastName}
        placeholder={t.contacts.fields.lastName}
        aria-label={t.contacts.fields.lastName}
        disabled={saving}
        onKeyDown={onKeyDown}
        onChange={(e) => setLastName(e.target.value)}
      />
      <button type="button" className="icon" title={t.common.save} aria-label={t.common.save} aria-busy={saving} onClick={() => void save()}>
        ✓
      </button>
      <button type="button" className="icon" title={t.common.cancel} aria-label={t.common.cancel} onClick={() => setEditing(false)}>
        ✕
      </button>
      {error && <small className="error">{error}</small>}
    </span>
  )
}
