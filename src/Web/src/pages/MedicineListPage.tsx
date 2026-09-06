import { useEffect, useMemo, useState } from 'react';
import { api, type Medicine } from '../api/client';
import { PageHeader } from '../components/PageHeader';
import { TableToolbar } from '../components/TableToolbar';

const subTabs = ['Medicine', 'Category', 'Generic', 'Type', 'Strength', 'Dosage', 'Route', 'Disposable'];

export function MedicineListPage() {
  const [medicines, setMedicines] = useState<Medicine[]>([]);
  const [search, setSearch] = useState('');
  const [pageSize, setPageSize] = useState(25);
  const [activeTab, setActiveTab] = useState('Medicine');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    api<Medicine[]>(`/api/medicines?search=${encodeURIComponent(search)}`)
      .then(setMedicines)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [search]);

  const visible = useMemo(() => medicines.slice(0, pageSize), [medicines, pageSize]);

  return (
    <div className="panel">
      <PageHeader title="Medicine Master" subtitle="Cardiology formulary — LOOM-style master list" />
      <div className="sub-tabs">
        {subTabs.map(tab => (
          <button
            key={tab}
            type="button"
            className={tab === activeTab ? 'sub-tab active' : tab === 'Medicine' ? 'sub-tab' : 'sub-tab soon'}
            onClick={() => tab === 'Medicine' && setActiveTab(tab)}
            disabled={tab !== 'Medicine'}
          >
            {tab}{tab !== 'Medicine' && ' · Soon'}
          </button>
        ))}
      </div>
      {loading && <div className="loading-bar" />}
      <TableToolbar
        pageSize={pageSize}
        onPageSizeChange={setPageSize}
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search name or generic"
        total={medicines.length}
      />
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr><th>Category</th><th>Name</th><th>Generic</th><th>Dosage</th><th>Route</th><th>Strength</th><th>Stock</th></tr>
          </thead>
          <tbody>
            {visible.map(m => (
              <tr key={m.id}>
                <td>{m.category}</td>
                <td>{m.name}</td>
                <td>{m.generic}</td>
                <td>{m.dosage}</td>
                <td>{m.route}</td>
                <td>{m.strength}</td>
                <td>{m.stock}</td>
              </tr>
            ))}
            {visible.length === 0 && !loading && (
              <tr><td colSpan={7} className="empty">No medicines found</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
