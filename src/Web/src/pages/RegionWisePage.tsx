import { useEffect, useState } from 'react';
import { api, type RegionWiseReport } from '../api/client';
import { PageHeader } from '../components/PageHeader';

export function RegionWisePage() {
  const [report, setReport] = useState<RegionWiseReport | null>(null);
  const [from, setFrom] = useState(() => new Date(Date.now() - 30 * 86400000).toISOString().slice(0, 10));
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const [loading, setLoading] = useState(false);

  async function generate() {
    setLoading(true);
    try {
      setReport(await api<RegionWiseReport>(`/api/reports/region-wise?from=${from}&to=${to}`));
    } finally { setLoading(false); }
  }

  useEffect(() => { generate(); }, []);

  const totals = report?.rows.reduce(
    (a, r) => ({ registered: a.registered + r.registered, opd: a.opd + r.opd, revenue: a.revenue + r.revenue }),
    { registered: 0, opd: 0, revenue: 0 },
  );

  return (
    <div className="panel">
      <PageHeader title="Region Wise Report" subtitle="Performance by branch / geographic region" />
      <div className="report-filters">
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
      <div className="table-wrap">
        <table className="data-table">
          <thead><tr><th>Region</th><th>Branch</th><th>Registered</th><th>OPD</th><th>Revenue</th></tr></thead>
          <tbody>
            {report?.rows.length ? report.rows.map(r => (
              <tr key={r.branchName}>
                <td>{r.region}</td>
                <td>{r.branchName}</td>
                <td>{r.registered}</td>
                <td>{r.opd}</td>
                <td>PKR {r.revenue.toLocaleString()}</td>
              </tr>
            )) : <tr><td colSpan={5} className="empty">No data available in table</td></tr>}
          </tbody>
          {totals && report && report.rows.length > 0 && (
            <tfoot>
              <tr>
                <td colSpan={2}><strong>Total</strong></td>
                <td>{totals.registered}</td>
                <td>{totals.opd}</td>
                <td>PKR {totals.revenue.toLocaleString()}</td>
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </div>
  );
}
