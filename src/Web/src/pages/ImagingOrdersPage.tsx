import { useEffect, useState } from 'react';
import { api, type ImagingOrder } from '../api/client';
import { PageHeader } from '../components/PageHeader';
import { TableToolbar } from '../components/TableToolbar';

export function ImagingOrdersPage() {
  const [orders, setOrders] = useState<ImagingOrder[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    api<ImagingOrder[]>('/api/imaging/orders')
      .then(setOrders)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const filtered = orders.filter(o => {
    const q = search.trim().toLowerCase();
    if (!q) return true;
    return o.patientName.toLowerCase().includes(q) || o.serviceName.toLowerCase().includes(q) || o.mrNumber.toLowerCase().includes(q);
  });

  return (
    <div className="panel">
      <PageHeader
        title="Imaging Orders"
        subtitle="Holter, Echo, and other imaging — delivery tracking"
      />
      {loading && <div className="loading-bar" />}
      <TableToolbar
        pageSize={25}
        onPageSizeChange={() => {}}
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search patient or study"
        total={filtered.length}
      />
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr><th>Patient</th><th>MRNo</th><th>Study</th><th>Charges</th><th>Delivery Date</th><th>Status</th><th>Ordered</th></tr>
          </thead>
          <tbody>
            {filtered.map(o => (
              <tr key={o.id}>
                <td>{o.patientName}</td>
                <td>{o.mrNumber}</td>
                <td>{o.serviceName}</td>
                <td>PKR {o.charges.toLocaleString()}</td>
                <td>{o.deliveryDate ?? '—'}</td>
                <td><span className="soon-badge">{o.status}</span></td>
                <td>{new Date(o.orderedAt).toLocaleDateString()}</td>
              </tr>
            ))}
            {filtered.length === 0 && !loading && (
              <tr><td colSpan={7} className="empty">No imaging orders — check in Holter or Echo from Patient Vault</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
