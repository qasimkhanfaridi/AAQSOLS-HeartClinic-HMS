import { useEffect, useState } from 'react';
import { api, type ReceptionCashFlowReport } from '../api/client';
import { PageHeader } from '../components/PageHeader';

function formatDate(iso: string) {
  const d = new Date(iso);
  return `${String(d.getDate()).padStart(2, '0')}-${String(d.getMonth() + 1).padStart(2, '0')}-${d.getFullYear()}`;
}

export function ReceptionCashFlowPage() {
  const [report, setReport] = useState<ReceptionCashFlowReport | null>(null);
  const [from, setFrom] = useState(() => new Date(Date.now() - 7 * 86400000).toISOString().slice(0, 10));
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const [loading, setLoading] = useState(false);

  async function generate() {
    setLoading(true);
    try {
      setReport(await api<ReceptionCashFlowReport>(`/api/reports/reception-cash-flow?from=${from}&to=${to}`));
    } finally { setLoading(false); }
  }

  useEffect(() => { generate(); }, []);

  return (
    <div className="panel">
      <PageHeader
        title="Reception Cash Flow"
        subtitle="Patient collections vs panel claims at reception challan"
      />
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
          <p className="table-meta">
            Patient collected: <strong>PKR {report.totalPatientCollected.toLocaleString()}</strong>
            {' · '}Panel claims: <strong>PKR {report.totalPanelAmount.toLocaleString()}</strong>
          </p>
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr><th>Date</th><th>Collected By</th><th>Patient</th><th>Challan</th><th>Patient Paid</th><th>Panel</th><th>Sub Total</th></tr>
              </thead>
              <tbody>
                {report.rows.length ? report.rows.map((r, i) => (
                  <tr key={i}>
                    <td>{formatDate(r.date)}</td>
                    <td>{r.collectedBy}</td>
                    <td>{r.patientName}</td>
                    <td>{r.challanNumber}</td>
                    <td>PKR {r.patientCollected.toLocaleString()}</td>
                    <td>PKR {r.panelAmount.toLocaleString()}</td>
                    <td>PKR {r.subTotal.toLocaleString()}</td>
                  </tr>
                )) : <tr><td colSpan={7} className="empty">No data available in table</td></tr>}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
