import type { ReactNode } from 'react'

type FormFieldProps = {
  label: string
  error?: string
  children: ReactNode
}

/** Label, input and error message. Pico shows the input in red when it has aria-invalid. */
export default function FormField({ label, error, children }: FormFieldProps) {
  return (
    <label>
      {label}
      {children}
      {error && <small>{error}</small>}
    </label>
  )
}