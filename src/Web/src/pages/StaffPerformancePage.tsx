import { useEffect, useMemo, useState } from 'react';
import { api, type StaffPerformanceReport } from '../api/client';
import { PageHeader } from '../components/PageHeader';
import { TableToolbar } from '../components/TableToolbar';

function formatDate(iso: string) {
  const d = new Date(iso + 'T00:00:00');
  const dd = String(d.getDate()).padStart(2, '0');
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  return `${dd}-${mm}-${d.getFullYear()}`;
}

export function StaffPerformancePage() {
  const [report, setReport] = useState<StaffPerformanceReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState(() => new Date().toISOString().slice(0, 10));
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const [search, setSearch] = useState('');
  const [pageSize, setPageSize] = useState(10);

  useEffect(() => {
    setLoading(true);
    api<StaffPerformanceReport>(`/api/reports/staff-performance?from=${from}&to=${to}`)
      .then(setReport)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [from, to]);

  const rows = useMemo(() => {
    const all = report?.rows ?? [];
    const q = search.trim().toLowerCase();
    const filtered = q ? all.filter(r => r.userName.toLowerCase().includes(q)) : all;
    return filtered.slice(0, pageSize);
  }, [report, search, pageSize]);

  const totals = report?.rows.reduce(
    (acc, r) => ({
      registered: acc.registered + r.registered,
      opd: acc.opd + r.opd,
      services: acc.services + r.services,
      visitedTotal: acc.visitedTotal + r.visitedTotal,
    }),
    { registered: 0, opd: 0, services: 0, visitedTotal: 0 },
  );

  return (
    <div className="panel">
      <PageHeader
        title="Staff Performance Details"
        subtitle={`Report period: ${formatDate(from)} to ${formatDate(to)}`}
        actions={
          <div className="date-range">
            <input type="date" value={from} onChange={e => setFrom(e.target.value)} />
            <span>to</span>
            <input type="date" value={to} onChange={e => setTo(e.target.value)} />
          </div>
        }
      />
      {loading && <div className="loading-bar" />}
      <TableToolbar
        pageSize={pageSize}
        onPageSizeChange={setPageSize}
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Filter by user name"
        total={report?.rows.length}
      />
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>User Name</th>
              <th>Registered</th>
              <th>OPD</th>
              <th>ER</th>
              <th>IPD</th>
              <th>Services</th>
              <th>Doctor</th>
              <th>Visited Total</th>
            </tr>
          </thead>
          <tbody>
            {rows.length > 0 ? rows.map(row => (
              <tr key={row.userName}>
                <td>{row.userName}</td>
                <td>{row.registered}</td>
                <td>{row.opd}</td>
                <td>{row.er}</td>
                <td>{row.ipd}</td>
                <td>{row.services}</td>
                <td>{row.doctor}</td>
                <td>{row.visitedTotal}</td>
              </tr>
            )) : (
              <tr><td colSpan={8} className="empty">No data available in table</td></tr>
            )}
          </tbody>
          {totals && report && report.rows.length > 0 && (
            <tfoot>
              <tr>
                <td><strong>Total</strong></td>
                <td>{totals.registered}</td>
                <td>{totals.opd}</td>
                <td>0</td>
                <td>0</td>
                <td>{totals.services}</td>
                <td>0</td>
                <td>{totals.visitedTotal}</td>
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </div>
  );
}
