import type { Sort } from '../hooks/useSort'

type SortableHeaderProps<K extends string> = {
  label: string
  sortKey: K
  sort: Sort<K>
  onSort: (key: K) => void
}

/** A column header you can click to sort. aria-sort tells screen readers the current order. */
export default function SortableHeader<K extends string>({ label, sortKey, sort, onSort }: SortableHeaderProps<K>) {
  const active = sort.key === sortKey
  const arrow = !active ? '↕' : sort.direction === 'asc' ? '▲' : '▼'

  return (
    <th aria-sort={active ? (sort.direction === 'asc' ? 'ascending' : 'descending') : undefined}>
      <button type="button" className={active ? 'sort active' : 'sort'} onClick={() => onSort(sortKey)}>
        {label}
        <span aria-hidden="true">{arrow}</span>
      </button>
    </th>
  )
}
