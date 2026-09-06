import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, type Doctor, type Patient, type ServiceGroup } from '../api/client';
import { PageHeader } from '../components/PageHeader';
import { TableToolbar } from '../components/TableToolbar';

interface CheckInLine { serviceId: string; serviceName: string; charges: number; deliveryDate?: string }

function formatRegistered(iso: string) {
  return new Date(iso).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function PatientVaultPage() {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [search, setSearch] = useState('');
  const [pageSize, setPageSize] = useState(10);
  const [branchScope, setBranchScope] = useState('Local');
  const [loading, setLoading] = useState(true);
  const [modalPatient, setModalPatient] = useState<Patient | null>(null);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [groups, setGroups] = useState<ServiceGroup[]>([]);
  const [checkInType, setCheckInType] = useState('WalkIn');
  const [destination, setDestination] = useState('InvestigationsDiagnostics');
  const [prescribedBy, setPrescribedBy] = useState('Doctor');
  const [doctorId, setDoctorId] = useState('');
  const [groupId, setGroupId] = useState('');
  const [serviceId, setServiceId] = useState('');
  const [lines, setLines] = useState<CheckInLine[]>([]);
  const [smsAlert, setSmsAlert] = useState('Patient');
  const [panelBillingMode, setPanelBillingMode] = useState('FullPanel');
  const [message, setMessage] = useState('');

  function load() {
    setLoading(true);
    const allBranches = branchScope === 'Inter Branch';
    api<Patient[]>(`/api/patients?search=${encodeURIComponent(search)}&allBranches=${allBranches}`)
      .then(setPatients)
      .catch(console.error)
      .finally(() => setLoading(false));
  }

  useEffect(() => { load(); }, [branchScope]);

  useEffect(() => {
    if (modalPatient) {
      api<Doctor[]>('/api/doctors').then(setDoctors);
      api<ServiceGroup[]>('/api/services').then(setGroups);
      setLines([]);
      setDoctorId('');
      setGroupId('');
      setServiceId('');
    }
  }, [modalPatient]);

  const visiblePatients = useMemo(() => patients.slice(0, pageSize), [patients, pageSize]);

  function addService() {
    const group = groups.find(g => g.id === groupId);
    const service = group?.services.find(s => s.id === serviceId);
    if (!service) return;
    const delivery = service.deliveryDays
      ? new Date(Date.now() + service.deliveryDays * 86400000).toISOString().slice(0, 10)
      : undefined;
    setLines([...lines, { serviceId: service.id, serviceName: service.name, charges: service.charges, deliveryDate: delivery }]);
    setServiceId('');
  }

  async function submitCheckIn() {
    if (!modalPatient || lines.length === 0) {
      setMessage('Add at least one service');
      return;
    }
    try {
      const result = await api<{ challanNumber: string; subTotal: number; patientPayable: number; panelPayable: number; panelBillingMode?: string }>('/api/check-ins', {
        method: 'POST',
        body: JSON.stringify({
          patientId: modalPatient.id,
          doctorId: doctorId || null,
          checkInType, destination, prescribedBy, smsAlert,
          panelBillingMode: modalPatient.patientType === 'Panel' ? panelBillingMode : null,
          serviceLines: lines.map(l => ({
            serviceId: l.serviceId,
            serviceName: l.serviceName,
            charges: l.charges,
            deliveryDate: l.deliveryDate,
          })),
        }),
      });
      setMessage(`Check-in complete. Challan ${result.challanNumber} — Total PKR ${result.subTotal.toLocaleString()} (Patient: ${result.patientPayable.toLocaleString()} · Panel: ${result.panelPayable.toLocaleString()})`);
      setModalPatient(null);
      setLines([]);
      load();
    } catch (err) {
      setMessage(err instanceof Error ? err.message : 'Check-in failed');
    }
  }

  const subTotal = lines.reduce((s, l) => s + l.charges, 0);
  const selectedGroup = groups.find(g => g.id === groupId);

  return (
    <div className="panel">
      <PageHeader
        title="Patient's Vault"
        actions={
          <>
            <button type="button" className="btn primary" onClick={load}>Advance Search</button>
            <button type="button" className="btn secondary" onClick={() => { setSearch(''); load(); }}>Reset & Refresh</button>
            <Link to="/patients/add" className="btn navy" style={{ textDecoration: 'none' }}>Add Patient</Link>
          </>
        }
      />
      <div className="radio-row">
        <span>Scope</span>
        {['Local', 'Inter Branch'].map(v => (
          <label key={v}>
            <input type="radio" checked={branchScope === v} onChange={() => setBranchScope(v)} /> {v}
          </label>
        ))}
        {branchScope === 'Inter Branch' && (
          <span className="table-meta">Showing patients across all branches</span>
        )}
      </div>
      {message && <div className={`flash ${message.includes('complete') ? 'success' : 'error'}`}>{message}</div>}
      {loading && <div className="loading-bar" />}
      <TableToolbar
        pageSize={pageSize}
        onPageSizeChange={setPageSize}
        search={search}
        onSearchChange={setSearch}
        onSearch={load}
        searchPlaceholder="Search name, MR, CNIC"
        total={patients.length}
      />
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              <th>Name</th>
              {branchScope === 'Inter Branch' && <th>Branch</th>}
              <th>MRNo</th>
              <th>CNIC/Passport</th>
              <th>Gender</th>
              <th>Registered On</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {visiblePatients.map(p => (
              <tr key={p.id}>
                <td>{p.firstName} ({p.patientType})</td>
                {branchScope === 'Inter Branch' && <td>{p.branchRegion ?? p.branchName ?? '—'}</td>}
                <td>{p.mrNumber}</td>
                <td>{p.cnicOrPassport}</td>
                <td>{p.gender}</td>
                <td>{formatRegistered(p.registeredOn)}</td>
                <td>
                  <div className="action-icons">
                    <button type="button" className="btn icon outline" title="View">👁</button>
                    <button type="button" className="btn icon outline" title="Edit">✎</button>
                    <button type="button" className="btn icon outline" title="Profile">👤</button>
                    <button type="button" className="btn icon outline" title="Print">🖨</button>
                    <button type="button" className="btn small primary" onClick={() => { setModalPatient(p); setMessage(''); }}>Check In</button>
                  </div>
                </td>
              </tr>
            ))}
            {visiblePatients.length === 0 && !loading && (
              <tr><td colSpan={6} className="empty">No patients found</td></tr>
            )}
          </tbody>
        </table>
      </div>

      {modalPatient && (
        <div className="modal-backdrop">
          <div className="modal">
            <div className="modal-header">
              <h3>Check In {modalPatient.firstName} ({modalPatient.patientType})</h3>
            </div>
            <div className="modal-body">
              <div className="radio-row">
                <span>Type *</span>
                {['WalkIn', 'Referral'].map(v => (
                  <label key={v}><input type="radio" checked={checkInType === v} onChange={() => setCheckInType(v)} /> {v === 'WalkIn' ? 'Walk-In' : 'Referral'}</label>
                ))}
              </div>
              <div className="radio-row">
                <span>Check In To *</span>
                {[
                  ['Department', 'Department'],
                  ['Doctor', 'Doctor'],
                  ['InvestigationsDiagnostics', 'Inves/Diagnostics'],
                ].map(([v, label]) => (
                  <label key={v}><input type="radio" checked={destination === v} onChange={() => setDestination(v)} /> {label}</label>
                ))}
              </div>
              <div className="form-row-2">
                <label>Package
                  <select disabled><option>Select Package</option></select>
                </label>
                <div className="radio-row" style={{ marginBottom: 0 }}>
                  <span>Prescribed By *</span>
                  {['Doctor', 'Self', 'Outdoor'].map(v => (
                    <label key={v}><input type="radio" checked={prescribedBy === v} onChange={() => setPrescribedBy(v)} /> {v}</label>
                  ))}
                </div>
              </div>
              <div className="form-row-2">
                <label>Doctor
                  <select value={doctorId} onChange={e => setDoctorId(e.target.value)}>
                    <option value="">Select doctor</option>
                    {doctors.map(d => <option key={d.id} value={d.id}>{d.fullName} | {d.specialty}</option>)}
                  </select>
                </label>
                <label>Service Group
                  <select value={groupId} onChange={e => { setGroupId(e.target.value); setServiceId(''); }}>
                    <option value="">Select Group</option>
                    {groups.map(g => <option key={g.id} value={g.id}>{g.name}</option>)}
                  </select>
                </label>
              </div>
              <label>Services
                <div className="rx-picker">
                  <select value={serviceId} onChange={e => setServiceId(e.target.value)}>
                    <option value="">Select service</option>
                    {selectedGroup?.services.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                  </select>
                  <button type="button" className="btn secondary" onClick={addService}>Add Service</button>
                </div>
              </label>
              <div className="table-wrap">
                <table className="data-table compact">
                  <thead><tr><th>Service Name</th><th>Charges</th><th>Delivery Date</th></tr></thead>
                  <tbody>
                    {lines.length === 0 ? (
                      <tr><td colSpan={3} className="empty">No services added</td></tr>
                    ) : lines.map(l => (
                      <tr key={l.serviceId}><td>{l.serviceName}</td><td>PKR {l.charges.toLocaleString()}</td><td>{l.deliveryDate ?? '—'}</td></tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <p style={{ margin: '.75rem 0' }}><strong>Sub Total:</strong> PKR {subTotal.toLocaleString()}</p>
              {modalPatient.patientType === 'Panel' && (
                <div className="radio-row">
                  <span>Panel Billing *</span>
                  {[
                    ['FullPanel', '100% Panel'],
                    ['SplitHalf', '50% / 50%'],
                    ['PanelClaim', 'Panel Claim'],
                  ].map(([v, label]) => (
                    <label key={v}>
                      <input type="radio" checked={panelBillingMode === v} onChange={() => setPanelBillingMode(v)} /> {label}
                    </label>
                  ))}
                  {modalPatient.corporatePanelName && (
                    <span className="table-meta">Panel: {modalPatient.corporatePanelName}</span>
                  )}
                </div>
              )}
              <div className="radio-row">
                <span>SMS Alert</span>
                {['Patient', 'Organization', 'Both', 'None'].map(v => (
                  <label key={v}><input type="radio" checked={smsAlert === v} onChange={() => setSmsAlert(v)} /> {v}</label>
                ))}
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn outline" onClick={() => setModalPatient(null)}>Cancel</button>
              <button type="button" className="btn primary" onClick={submitCheckIn}>Submit</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
