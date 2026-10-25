import { useEffect } from 'react'

/**
 * Shows the browser's "leave page?" warning on reload or tab close while `active` is true.
 * Browsers use their own text here; a custom message is not allowed.
 */
export function useUnsavedChangesWarning(active: boolean) {
  useEffect(() => {
    if (!active) return

    const warn = (event: BeforeUnloadEvent) => event.preventDefault()
    window.addEventListener('beforeunload', warn)
    return () => window.removeEventListener('beforeunload', warn)
  }, [active])
}