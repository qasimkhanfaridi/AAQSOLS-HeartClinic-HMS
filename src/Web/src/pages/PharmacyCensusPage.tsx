import { useEffect, useState } from 'react';
import { api, type PharmacyCensusReport, type ReportFilters } from '../api/client';
import { PageHeader } from '../components/PageHeader';

function formatDate(iso: string) {
  const [y, m, d] = iso.split('-');
  return `${d}-${m}-${y}`;
}

export function PharmacyCensusPage() {
  const [filters, setFilters] = useState<ReportFilters | null>(null);
  const [report, setReport] = useState<PharmacyCensusReport | null>(null);
  const [from, setFrom] = useState(() => new Date(Date.now() - 7 * 86400000).toISOString().slice(0, 10));
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const [storeId, setStoreId] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api<ReportFilters>('/api/reports/filters').then(setFilters).catch(console.error);
  }, []);

  async function search() {
    setLoading(true);
    try {
      const data = await api<PharmacyCensusReport>(`/api/reports/pharmacy-census?from=${from}&to=${to}`);
      setReport(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { search(); }, []);

  const totals = report?.rows.reduce(
    (acc, r) => ({ males: acc.males + r.males, females: acc.females + r.females, total: acc.total + r.total }),
    { males: 0, females: 0, total: 0 },
  );

  return (
    <div className="panel">
      <PageHeader title="Pharmacy Census Report" subtitle="Patients with prescriptions by date and gender" />

      <div className="report-filters">
        <div className="form-row-2">
          <label>Date From
            <input type="date" value={from} onChange={e => setFrom(e.target.value)} />
          </label>
          <label>Date To
            <input type="date" value={to} onChange={e => setTo(e.target.value)} />
          </label>
        </div>
        <label>Pharmacy / Store
          <select value={storeId} onChange={e => setStoreId(e.target.value)}>
            {filters?.stores.map(s => <option key={s.id || 'main'} value={s.id}>{s.name}</option>)}
          </select>
        </label>
        <div className="actions">
          <button type="button" className="btn primary" onClick={search} disabled={loading}>
            {loading ? 'Searching...' : 'Search'}
          </button>
        </div>
      </div>

      {loading && <div className="loading-bar" />}

      <div className="table-wrap" style={{ marginTop: '1rem' }}>
        <table className="data-table">
          <thead><tr><th>Date</th><th>Males</th><th>Females</th><th>Total</th></tr></thead>
          <tbody>
            {report?.rows.length ? report.rows.map(r => (
              <tr key={r.date}>
                <td>{formatDate(r.date)}</td>
                <td>{r.males}</td>
                <td>{r.females}</td>
                <td>{r.total}</td>
              </tr>
            )) : (
              <tr><td colSpan={4} className="empty">No data available in table</td></tr>
            )}
          </tbody>
          {totals && report && report.rows.length > 0 && (
            <tfoot>
              <tr>
                <td><strong>Total</strong></td>
                <td>{totals.males}</td>
                <td>{totals.females}</td>
                <td>{totals.total}</td>
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </div>
  );
}
