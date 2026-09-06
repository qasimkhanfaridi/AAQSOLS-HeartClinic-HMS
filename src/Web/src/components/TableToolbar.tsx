interface TableToolbarProps {
  pageSize: number;
  onPageSizeChange: (size: number) => void;
  search: string;
  onSearchChange: (value: string) => void;
  searchPlaceholder?: string;
  onSearch?: () => void;
  total?: number;
}

export function TableToolbar({
  pageSize,
  onPageSizeChange,
  search,
  onSearchChange,
  searchPlaceholder = 'Search...',
  onSearch,
  total,
}: TableToolbarProps) {
  return (
    <div className="table-toolbar">
      <div className="table-toolbar-left">
        <label className="inline-label">
          Show
          <select
            className="inline-select"
            value={pageSize}
            onChange={e => onPageSizeChange(Number(e.target.value))}
          >
            {[10, 25, 50, 100].map(n => (
              <option key={n} value={n}>{n}</option>
            ))}
          </select>
          entries
        </label>
        {total !== undefined && (
          <span className="table-meta">{total} record{total === 1 ? '' : 's'}</span>
        )}
      </div>
      <div className="table-toolbar-right">
        <label className="inline-label search-label">
          Search:
          <input
            className="inline-search"
            placeholder={searchPlaceholder}
            value={search}
            onChange={e => onSearchChange(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && onSearch?.()}
          />
        </label>
      </div>
    </div>
  );
}
