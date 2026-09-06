import { useEffect, useState } from 'react';
import { api, type ReportFilters, type ServicesCashFlowReport } from '../api/client';
import { PageHeader } from '../components/PageHeader';

function formatDate(iso: string) {
  const d = new Date(iso);
  return `${String(d.getDate()).padStart(2, '0')}-${String(d.getMonth() + 1).padStart(2, '0')}-${d.getFullYear()}`;
}

export function ServicesCashFlowPage() {
  const [filters, setFilters] = useState<ReportFilters | null>(null);
  const [report, setReport] = useState<ServicesCashFlowReport | null>(null);
  const [from, setFrom] = useState(() => new Date(Date.now() - 7 * 86400000).toISOString().slice(0, 10));
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const [serviceId, setServiceId] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => { api<ReportFilters>('/api/reports/filters').then(setFilters).catch(console.error); }, []);

  async function generate() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ from, to });
      if (serviceId) params.set('serviceId', serviceId);
      setReport(await api<ServicesCashFlowReport>(`/api/reports/services-cash-flow?${params}`));
    } finally { setLoading(false); }
  }

  useEffect(() => { generate(); }, []);

  return (
    <div className="panel">
      <PageHeader title="Services Cash Flow" subtitle="All OPD and diagnostic service revenue" />
      <div className="report-filters">
        <label>Service
          <select value={serviceId} onChange={e => setServiceId(e.target.value)}>
            <option value="">All Services</option>
            {filters?.services.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
        </label>
        <div className="filters">
          <div className="date-range">
            <input type="date" value={from} onChange={e => setFrom(e.target.value)} />
            <span>to</span>
            <input type="date" value={to} onChange={e => setTo(e.target.value)} />
          </div>
          <button type="button" className="btn primary" onClick={generate} disabled={loading}>Generate Report</button>
        </div>
      </div>
      {loading && <div className="loading-bar" />}
      {report && (
        <>
          <p className="table-meta">Grand total: <strong>PKR {report.grandTotal.toLocaleString()}</strong></p>
          <div className="table-wrap">
            <table className="data-table">
              <thead><tr><th>Date</th><th>Service</th><th>Patient</th><th>Type</th><th>Charges</th></tr></thead>
              <tbody>
                {report.rows.length ? report.rows.map((r, i) => (
                  <tr key={i}>
                    <td>{formatDate(r.date)}</td>
                    <td>{r.serviceName}</td>
                    <td>{r.patientName}</td>
                    <td>{r.patientType}</td>
                    <td>PKR {r.charges.toLocaleString()}</td>
                  </tr>
                )) : <tr><td colSpan={5} className="empty">No data available in table</td></tr>}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
