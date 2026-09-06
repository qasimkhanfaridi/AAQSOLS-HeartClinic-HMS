import { useEffect, useState } from 'react';
import { api, type LabOrder } from '../api/client';
import { PageHeader } from '../components/PageHeader';
import { TableToolbar } from '../components/TableToolbar';

const statusTabs = [
  { key: 'PendingPayment', label: 'Pending Payments' },
  { key: 'PaymentVerified', label: 'Ready for Sample' },
  { key: 'SampleCollected', label: 'Sample Collected' },
];

export function LabPendingPaymentsPage() {
  const [status, setStatus] = useState('PendingPayment');
  const [orders, setOrders] = useState<LabOrder[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('');

  function load() {
    setLoading(true);
    api<LabOrder[]>(`/api/lab/orders?status=${status}`)
      .then(setOrders)
      .catch(console.error)
      .finally(() => setLoading(false));
  }

  useEffect(() => { load(); }, [status]);

  async function verifyPayment(id: string) {
    await api(`/api/lab/orders/${id}/verify-payment`, { method: 'POST' });
    setMessage('Payment verified — ready for sample collection');
    load();
  }

  async function collectSample(id: string) {
    await api(`/api/lab/orders/${id}/collect-sample`, { method: 'POST' });
    setMessage('Sample collected');
    load();
  }

  const filtered = orders.filter(o => {
    const q = search.trim().toLowerCase();
    if (!q) return true;
    return o.patientName.toLowerCase().includes(q) || o.mrNumber.toLowerCase().includes(q) || o.serviceName.toLowerCase().includes(q);
  });

  return (
    <div className="panel">
      <PageHeader
        title="Lab Investigations"
        subtitle="Pending payment → verify → sample collection (LOOM lab workflow)"
      />
      <div className="sub-tabs">
        {statusTabs.map(t => (
          <button
            key={t.key}
            type="button"
            className={status === t.key ? 'sub-tab active' : 'sub-tab'}
            onClick={() => setStatus(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>
      {message && <div className="flash success">{message}</div>}
      {loading && <div className="loading-bar" />}
      <TableToolbar
        pageSize={25}
        onPageSizeChange={() => {}}
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search patient, MR, test"
        total={filtered.length}
      />
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr><th>Patient</th><th>MRNo</th><th>Test</th><th>Charges</th><th>Ordered</th><th>Status</th><th>Action</th></tr>
          </thead>
          <tbody>
            {filtered.map(o => (
              <tr key={o.id}>
                <td>{o.patientName}</td>
                <td>{o.mrNumber}</td>
                <td>{o.serviceName}</td>
                <td>PKR {o.charges.toLocaleString()}</td>
                <td>{new Date(o.orderedAt).toLocaleDateString()}</td>
                <td><span className="soon-badge">{o.status}</span></td>
                <td>
                  {o.status === 'PendingPayment' && (
                    <button type="button" className="btn small primary" onClick={() => verifyPayment(o.id)}>Verify Payment</button>
                  )}
                  {o.status === 'PaymentVerified' && (
                    <button type="button" className="btn small secondary" onClick={() => collectSample(o.id)}>Collect Sample</button>
                  )}
                </td>
              </tr>
            ))}
            {filtered.length === 0 && !loading && (
              <tr><td colSpan={7} className="empty">No lab orders in this queue</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
