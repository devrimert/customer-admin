import { useMemo, useState } from 'react'
import { locale } from '../i18n'

export type SortDirection = 'asc' | 'desc'
export type Sort<K extends string> = { key: K; direction: SortDirection }

type SortValue = string | number | null | undefined

// Compares text the way people expect in German: "ä" next to "a", "10" after "9"
const collator = new Intl.Collator(locale, { sensitivity: 'base', numeric: true })

const isEmpty = (value: SortValue) => value === null || value === undefined || value === ''

/**
 * Sorts a list in the browser. We always load the whole result (no paging), so this is enough,
 * and it keeps working while the user searches. Clicking the same column again flips the order.
 * `pinFirst` keeps some rows on top whatever the sort is, e.g. the primary contact.
 */
export function useSort<T, K extends string>(
  items: readonly T[] | undefined,
  getValue: (item: T, key: K) => SortValue,
  initial: Sort<K>,
  pinFirst?: (item: T) => boolean,
) {
  const [sort, setSort] = useState(initial)

  const sorted = useMemo(() => {
    if (!items) return undefined

    const factor = sort.direction === 'asc' ? 1 : -1
    return [...items].sort((a, b) => {
      if (pinFirst) {
        const pinned = Number(pinFirst(b)) - Number(pinFirst(a))
        if (pinned !== 0) return pinned
      }

      const valueA = getValue(a, sort.key)
      const valueB = getValue(b, sort.key)

      // Empty values always go to the end, in both directions
      if (isEmpty(valueA) || isEmpty(valueB)) {
        return Number(isEmpty(valueA)) - Number(isEmpty(valueB))
      }

      if (typeof valueA === 'string' && typeof valueB === 'string') {
        return collator.compare(valueA, valueB) * factor
      }
      return (Number(valueA) - Number(valueB)) * factor
    })
  }, [items, sort, getValue, pinFirst])

  const toggle = (key: K) =>
    setSort((current) =>
      current.key === key
        ? { key, direction: current.direction === 'asc' ? 'desc' : 'asc' }
        : { key, direction: 'asc' },
    )

  return { sorted, sort, toggle }
}
