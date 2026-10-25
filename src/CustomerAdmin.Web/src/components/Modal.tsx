import { useEffect, useRef, type ReactNode } from 'react'
import { t } from '../i18n'

type ModalProps = {
  title: string
  onClose: () => void
  children: ReactNode
  footer?: ReactNode
}

/**
 * A modal based on the native <dialog> element. It opens when it is rendered.
 * Closing is always handed to the parent, so the parent can ask about unsaved changes first.
 */
export default function Modal({ title, onClose, children, footer }: ModalProps) {
  const ref = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialog = ref.current
    if (dialog && !dialog.open) {
      dialog.showModal()
    }
    return () => dialog?.close()
  }, [])

  return (
    <dialog
      ref={ref}
      // Esc key: don't close right away, let the parent decide
      onCancel={(e) => {
        e.preventDefault()
        onClose()
      }}
      // A click on the dark area around the box
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose()
      }}
    >
      <article>
        <header>
          <button type="button" rel="prev" aria-label={t.common.close} onClick={onClose} />
          <h3>{title}</h3>
        </header>
        {children}
        {footer && <footer>{footer}</footer>}
      </article>
    </dialog>
  )
}