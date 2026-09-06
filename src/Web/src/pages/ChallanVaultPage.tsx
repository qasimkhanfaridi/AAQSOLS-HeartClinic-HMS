import { useEffect, useState } from 'react';
import { api, type ChallanRecord } from '../api/client';
import { PageHeader } from '../components/PageHeader';
import { TableToolbar } from '../components/TableToolbar';

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

export function ChallanVaultPage() {
  const [challans, setChallans] = useState<ChallanRecord[]>([]);
  const [search, setSearch] = useState('');
  const [from, setFrom] = useState(() => new Date(Date.now() - 30 * 86400000).toISOString().slice(0, 10));
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const [loading, setLoading] = useState(true);

  function load() {
    setLoading(true);
    api<ChallanRecord[]>(`/api/challans?search=${encodeURIComponent(search)}&from=${from}&to=${to}`)
      .then(setChallans)
      .catch(console.error)
      .finally(() => setLoading(false));
  }

  useEffect(() => { load(); }, []);

  return (
    <div className="panel">
      <PageHeader title="Challan Vault" subtitle="All issued challans — search and reprint reference" />
      <div className="report-filters">
        <div className="filters">
          <div className="date-range">
            <input type="date" value={from} onChange={e => setFrom(e.target.value)} />
            <span>to</span>
            <input type="date" value={to} onChange={e => setTo(e.target.value)} />
          </div>
          <button type="button" className="btn primary" onClick={load}>Search</button>
          <button type="button" className="btn outline" onClick={() => window.print()}>Print</button>
        </div>
      </div>
      {loading && <div className="loading-bar" />}
      <TableToolbar
        pageSize={25}
        onPageSizeChange={() => {}}
        search={search}
        onSearchChange={setSearch}
        onSearch={load}
        searchPlaceholder="Challan no, patient, MR"
        total={challans.length}
      />
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>Challan No</th><th>Patient</th><th>MRNo</th><th>Services</th>
              <th>Sub Total</th><th>Patient</th><th>Panel</th><th>Issued</th><th>By</th>
            </tr>
          </thead>
          <tbody>
            {challans.map(c => (
              <tr key={c.id}>
                <td><strong>{c.challanNumber}</strong></td>
                <td>{c.patientName}</td>
                <td>{c.mrNumber}</td>
                <td>{c.services}</td>
                <td>PKR {c.subTotal.toLocaleString()}</td>
                <td>PKR {c.patientPayable.toLocaleString()}</td>
                <td>PKR {c.panelPayable.toLocaleString()}</td>
                <td>{formatDate(c.issuedAt)}</td>
                <td>{c.issuedBy}</td>
              </tr>
            ))}
            {challans.length === 0 && !loading && (
              <tr><td colSpan={9} className="empty">No challans in selected period</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
