import { useEffect, useState } from 'react'

/**
 * Returns the value only after it has stopped changing for `delayMs`.
 * Used for search-as-you-type, so we don't send a request on every key press.
 */
export function useDebouncedValue<T>(value: T, delayMs: number): T {
  const [debounced, setDebounced] = useState(value)

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs)
    // Runs before the next effect: a new key press cancels the old timer
    return () => clearTimeout(timer)
  }, [value, delayMs])

  return debounced
}