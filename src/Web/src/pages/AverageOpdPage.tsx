import { useEffect, useState } from 'react';
import { api, type AverageOpdReport } from '../api/client';
import { PageHeader } from '../components/PageHeader';

function formatDate(iso: string) {
  const [y, m, d] = iso.split('-');
  return `${d}-${m}-${y}`;
}

export function AverageOpdPage() {
  const [report, setReport] = useState<AverageOpdReport | null>(null);
  const [from, setFrom] = useState(() => new Date(Date.now() - 30 * 86400000).toISOString().slice(0, 10));
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const [loading, setLoading] = useState(false);

  async function generate() {
    setLoading(true);
    try {
      setReport(await api<AverageOpdReport>(`/api/reports/average-opd?from=${from}&to=${to}`));
    } finally { setLoading(false); }
  }

  useEffect(() => { generate(); }, []);

  return (
    <div className="panel">
      <PageHeader title="Average OPD Stats" subtitle="Daily OPD volume and clinic throughput" />
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
      {report && (
        <>
          <div className="form-row-2" style={{ margin: '1rem 0' }}>
            <div className="summary-card"><span>Total OPD: <strong>{report.totalOpd}</strong></span></div>
            <div className="summary-card"><span>Avg / day: <strong>{report.averageOpdPerDay}</strong></span></div>
            <div className="summary-card">
              <span>Peak day: <strong>{report.peakDayCount}</strong>
                {report.peakDate && ` (${formatDate(report.peakDate)})`}
              </span>
            </div>
          </div>
          <div className="table-wrap">
            <table className="data-table">
              <thead><tr><th>Date</th><th>OPD Count</th></tr></thead>
              <tbody>
                {report.dailyRows.length ? report.dailyRows.map(r => (
                  <tr key={r.date}>
                    <td>{formatDate(r.date)}</td>
                    <td>{r.opdCount}</td>
                  </tr>
                )) : <tr><td colSpan={2} className="empty">No OPD data in selected period</td></tr>}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
