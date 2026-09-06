import { useEffect, useState } from 'react';
import { api, type LabCashFlowReport, type ReportFilters } from '../api/client';
import { PageHeader } from '../components/PageHeader';

function formatDate(iso: string) {
  const d = new Date(iso);
  const dd = String(d.getDate()).padStart(2, '0');
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  return `${dd}-${mm}-${d.getFullYear()}`;
}

export function LabCashFlowPage() {
  const [filters, setFilters] = useState<ReportFilters | null>(null);
  const [report, setReport] = useState<LabCashFlowReport | null>(null);
  const [loading, setLoading] = useState(false);
  const [from, setFrom] = useState(() => new Date(Date.now() - 7 * 86400000).toISOString().slice(0, 10));
  const [to, setTo] = useState(() => new Date().toISOString().slice(0, 10));
  const [serviceId, setServiceId] = useState('');
  const [doctorId, setDoctorId] = useState('');
  const [departmentId, setDepartmentId] = useState('');
  const [patientTypeId, setPatientTypeId] = useState('');
  const [orderType, setOrderType] = useState('All');
  const [reportType, setReportType] = useState('Detailed');

  useEffect(() => {
    api<ReportFilters>('/api/reports/filters').then(setFilters).catch(console.error);
  }, []);

  async function generate() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ from, to, orderType, reportType });
      if (serviceId) params.set('serviceId', serviceId);
      if (doctorId) params.set('doctorId', doctorId);
      if (departmentId) params.set('departmentId', departmentId);
      if (patientTypeId) params.set('patientTypeId', patientTypeId);
      const data = await api<LabCashFlowReport>(`/api/reports/lab-cash-flow?${params}`);
      setReport(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { generate(); }, []);

  return (
    <div className="panel">
      <PageHeader title="Lab Cash Flow" subtitle="Diagnostic and lab service revenue — LOOM Accounts report" />

      <div className="report-filters">
        <div className="form-row-2">
          <label>Service
            <select value={serviceId} onChange={e => setServiceId(e.target.value)}>
              <option value="">All Services</option>
              {filters?.services.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
          </label>
          <label>Department
            <select value={departmentId} onChange={e => setDepartmentId(e.target.value)}>
              <option value="">All Departments</option>
              {filters?.departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
          </label>
        </div>
        <div className="form-row-2">
          <label>Doctor
            <select value={doctorId} onChange={e => setDoctorId(e.target.value)}>
              <option value="">All Doctors</option>
              {filters?.doctors.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
          </label>
          <label>Misc Services
            <select disabled><option>All</option></select>
          </label>
        </div>

        <div className="radio-row">
          <span>Order Type</span>
          {['All', 'OPD/Lab', 'OPD', 'LAB', 'ER/IPD'].map(v => (
            <label key={v}>
              <input type="radio" checked={orderType === v} onChange={() => setOrderType(v)} /> {v}
            </label>
          ))}
        </div>
        <div className="radio-row">
          <span>Patient Type</span>
          <label><input type="radio" checked={patientTypeId === ''} onChange={() => setPatientTypeId('')} /> All</label>
          {filters?.patientTypes.map(t => (
            <label key={t.id}>
              <input type="radio" checked={patientTypeId === t.id} onChange={() => setPatientTypeId(t.id)} /> {t.name}
            </label>
          ))}
        </div>
        <div className="radio-row">
          <span>Report Type</span>
          {['Detailed', 'Summary'].map(v => (
            <label key={v}>
              <input type="radio" checked={reportType === v} onChange={() => setReportType(v)} /> {v}
            </label>
          ))}
        </div>

        <div className="filters" style={{ marginTop: '.5rem' }}>
          <div className="date-range">
            <input type="date" value={from} onChange={e => setFrom(e.target.value)} />
            <span>to</span>
            <input type="date" value={to} onChange={e => setTo(e.target.value)} />
          </div>
          <button type="button" className="btn primary" onClick={generate} disabled={loading}>
            {loading ? 'Generating...' : 'Generate Report'}
          </button>
          <button type="button" className="btn outline" disabled title="PDF export — Phase 2">Pdf ▾</button>
        </div>
      </div>

      {loading && <div className="loading-bar" />}

      {report && (
        <>
          <p className="table-meta" style={{ margin: '1rem 0 .5rem' }}>
            {report.reportType} report · Grand total: <strong>PKR {report.grandTotal.toLocaleString()}</strong>
          </p>
          <div className="table-wrap">
            {report.reportType === 'Summary' ? (
              <table className="data-table">
                <thead><tr><th>Service Name</th><th>Count</th><th>Total Charges</th></tr></thead>
                <tbody>
                  {report.summaryRows?.length ? report.summaryRows.map(r => (
                    <tr key={r.serviceName}>
                      <td>{r.serviceName}</td>
                      <td>{r.count}</td>
                      <td>PKR {r.totalCharges.toLocaleString()}</td>
                    </tr>
                  )) : (
                    <tr><td colSpan={3} className="empty">No data available in table</td></tr>
                  )}
                </tbody>
              </table>
            ) : (
              <table className="data-table">
                <thead>
                  <tr><th>Date</th><th>Patient</th><th>MRNo</th><th>Service</th><th>Doctor</th><th>Type</th><th>Charges</th></tr>
                </thead>
                <tbody>
                  {report.detailedRows?.length ? report.detailedRows.map((r, i) => (
                    <tr key={i}>
                      <td>{formatDate(r.date)}</td>
                      <td>{r.patientName}</td>
                      <td>{r.mrNumber}</td>
                      <td>{r.serviceName}</td>
                      <td>{r.doctorName ?? '—'}</td>
                      <td>{r.patientType}</td>
                      <td>PKR {r.charges.toLocaleString()}</td>
                    </tr>
                  )) : (
                    <tr><td colSpan={7} className="empty">No data available in table</td></tr>
                  )}
                </tbody>
              </table>
            )}
          </div>
        </>
      )}
    </div>
  );
}
