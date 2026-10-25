import { useState, type KeyboardEvent } from 'react'
import type { ZodType } from 'zod'
import { errorMessage } from '../api/errors'
import { t } from '../i18n'

type InlineEditProps = {
  label: string
  value: string | null | undefined
  // The same zod rule as in the form
  schema: ZodType<string>
  type?: 'text' | 'email' | 'tel'
  multiline?: boolean
  onSave: (value: string | null) => Promise<unknown>
}

/** Shows a value with a pencil. Click it to change just this one field. */
export default function InlineEdit({ label, value, schema, type = 'text', multiline, onSave }: InlineEditProps) {
  const [editing, setEditing] = useState(false)
  const [draft, setDraft] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const start = () => {
    setDraft(value ?? '')
    setError(null)
    setEditing(true)
  }

  const save = async () => {
    const result = schema.safeParse(draft)
    if (!result.success) {
      setError(result.error.issues[0].message)
      return
    }

    setSaving(true)
    try {
      await onSave(result.data || null)
      setEditing(false)
    } catch (e) {
      setError(errorMessage(e))
    } finally {
      setSaving(false)
    }
  }

  // Enter saves, Esc cancels. In a text area, Shift+Enter makes a new line.
  const onKeyDown = (e: KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      void save()
    } else if (e.key === 'Escape') {
      setEditing(false)
    }
  }

  if (!editing) {
    return (
      <span className="inline-edit">
        <span className="value">{value || t.common.empty}</span>
        <button type="button" className="icon" title={t.common.edit} aria-label={`${label}: ${t.common.edit}`} onClick={start}>
          ✎
        </button>
      </span>
    )
  }

  const inputProps = {
    value: draft,
    'aria-label': label,
    'aria-invalid': error ? true : undefined,
    disabled: saving,
    autoFocus: true,
    onKeyDown,
  }

  return (
    <span className="inline-edit editing">
      {multiline ? (
        <textarea rows={3} {...inputProps} onChange={(e) => setDraft(e.target.value)} />
      ) : (
        <input type={type} {...inputProps} onChange={(e) => setDraft(e.target.value)} />
      )}
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