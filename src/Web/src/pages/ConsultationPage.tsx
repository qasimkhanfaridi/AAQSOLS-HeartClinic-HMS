import { useEffect, useState } from 'react';
import { api, type ActiveCheckIn, type Consultation, type Medicine } from '../api/client';
import { PageHeader } from '../components/PageHeader';
import { TableToolbar } from '../components/TableToolbar';

const sections = [
  'OtherComplaints',
  'ExamFindings',
  'PrimaryDiagnosis',
  'Diagnostics',
  'Investigations',
  'Medicines',
  'Instructions',
  'FollowUps',
  'Advice',
  'SecondaryDiagnosis',
] as const;

type Section = typeof sections[number];

function sectionLabel(s: Section) {
  return s.replace(/([A-Z])/g, ' $1').trim();
}

export function ConsultationPage() {
  const [queue, setQueue] = useState<ActiveCheckIn[]>([]);
  const [selected, setSelected] = useState<ActiveCheckIn | null>(null);
  const [consultation, setConsultation] = useState<Consultation | null>(null);
  const [activeSection, setActiveSection] = useState<Section>('Advice');
  const [entries, setEntries] = useState<Record<string, string>>({});
  const [medicines, setMedicines] = useState<Medicine[]>([]);
  const [selectedMed, setSelectedMed] = useState('');
  const [prescriptions, setPrescriptions] = useState<{ medicineId?: string; medicineName: string; dosage?: string; strength?: string }[]>([]);
  const [referToCategory, setReferToCategory] = useState(false);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [pageSize, setPageSize] = useState(10);

  function refreshQueue() {
    setLoading(true);
    api<ActiveCheckIn[]>('/api/check-ins/active')
      .then(setQueue)
      .catch(console.error)
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    refreshQueue();
    api<Medicine[]>('/api/medicines').then(setMedicines).catch(console.error);
  }, []);

  async function openConsult(item: ActiveCheckIn) {
    setSelected(item);
    setActiveSection('Advice');
    const data = await api<Consultation>(`/api/consultations/${item.id}`);
    setConsultation(data);
    const map: Record<string, string> = {};
    data.entries.forEach(e => { map[e.sectionType] = e.content; });
    setEntries(map);
    setPrescriptions(data.prescriptions.map(p => ({
      medicineId: undefined,
      medicineName: p.medicineName,
      dosage: p.dosage,
      strength: p.strength,
    })));
  }

  function addPrescription() {
    const med = medicines.find(m => m.id === selectedMed);
    if (!med) return;
    setPrescriptions([...prescriptions, { medicineId: med.id, medicineName: med.name, dosage: med.dosage, strength: med.strength }]);
    setSelectedMed('');
    setActiveSection('Medicines');
  }

  function removePrescription(index: number) {
    setPrescriptions(prescriptions.filter((_, i) => i !== index));
  }

  async function save(action: string) {
    if (!selected) return;
    await api(`/api/consultations/${selected.id}`, {
      method: 'POST',
      body: JSON.stringify({
        referToCategory,
        action,
        entries: sections.filter(s => entries[s]).map(s => ({ sectionType: s, content: entries[s] })),
        prescriptions,
      }),
    });
    setSelected(null);
    setConsultation(null);
    refreshQueue();
  }

  if (selected && consultation) {
    const adviceHistory = entries.Advice
      ? [{ advice: entries.Advice, doctor: selected.doctorName ?? 'Doctor', time: new Date().toLocaleTimeString() }]
      : [];

    return (
      <div className="consult-layout">
        <aside className="consult-nav">
          {sections.map(s => (
            <button
              key={s}
              type="button"
              className={activeSection === s ? 'consult-nav-item active' : 'consult-nav-item'}
              onClick={() => setActiveSection(s)}
            >
              {sectionLabel(s)}
            </button>
          ))}
        </aside>
        <div className="consult-main">
          <h2>{consultation.patientName} — {sectionLabel(activeSection)}</h2>

          {activeSection === 'Medicines' ? (
            <div className="rx-block">
              <h4>Prescription</h4>
              <div className="rx-picker">
                <select value={selectedMed} onChange={e => setSelectedMed(e.target.value)}>
                  <option value="">Select medicine</option>
                  {medicines.map(m => <option key={m.id} value={m.id}>{m.name}</option>)}
                </select>
                <button type="button" className="btn secondary" onClick={addPrescription}>Add to Rx</button>
              </div>
            </div>
          ) : activeSection === 'Advice' ? (
            <>
              <label>Advice
                <textarea rows={4} value={entries.Advice ?? ''} onChange={e => setEntries({ ...entries, Advice: e.target.value })} placeholder="Enter clinical advice..." />
              </label>
              <div className="table-wrap" style={{ marginTop: '1rem' }}>
                <table className="data-table compact">
                  <thead><tr><th>Advice</th><th>Doctor</th><th>Time</th></tr></thead>
                  <tbody>
                    {adviceHistory.length === 0 ? (
                      <tr><td colSpan={3} className="empty">No advice recorded yet</td></tr>
                    ) : adviceHistory.map((row, i) => (
                      <tr key={i}><td>{row.advice}</td><td>{row.doctor}</td><td>{row.time}</td></tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          ) : (
            <label>{sectionLabel(activeSection)}
              <textarea
                rows={6}
                value={entries[activeSection] ?? ''}
                onChange={e => setEntries({ ...entries, [activeSection]: e.target.value })}
                placeholder={`Enter ${sectionLabel(activeSection).toLowerCase()}...`}
              />
            </label>
          )}

          <div className="consult-bottom-bar">
            <label style={{ margin: 0, display: 'inline-flex', alignItems: 'center', gap: '.4rem', fontWeight: 500 }}>
              <input type="checkbox" checked={referToCategory} onChange={e => setReferToCategory(e.target.checked)} />
              Refer to Category
            </label>
            <div className="actions">
              <button type="button" className="btn outline" onClick={() => { setSelected(null); setConsultation(null); }}>Back</button>
              <button type="button" className="btn secondary" onClick={() => save('Hold')}>Hold</button>
              <button type="button" className="btn primary" onClick={() => save('Refer')}>Refer</button>
              <button type="button" className="btn navy" onClick={() => save('Consult')}>Consult</button>
            </div>
          </div>
        </div>
        <aside className="consult-summary">
          <h4 style={{ margin: '0 0 .75rem', fontSize: '.9rem' }}>Visit Summary</h4>
          {prescriptions.length === 0 && !entries.Advice && (
            <p className="table-meta">Add medicines or advice to build summary</p>
          )}
          {prescriptions.map((p, i) => (
            <div key={i} className="summary-card">
              <span>{p.medicineName} {p.strength && `(${p.strength})`}</span>
              <button type="button" className="remove" onClick={() => removePrescription(i)} title="Remove">×</button>
            </div>
          ))}
          {entries.Advice && (
            <div className="summary-card">
              <span>Advice: {entries.Advice}</span>
            </div>
          )}
          {entries.PrimaryDiagnosis && (
            <div className="summary-card">
              <span>Dx: {entries.PrimaryDiagnosis}</span>
            </div>
          )}
        </aside>
      </div>
    );
  }

  const filtered = queue.filter(q => {
    const term = search.trim().toLowerCase();
    if (!term) return true;
    return q.patientName.toLowerCase().includes(term) || q.mrNumber.toLowerCase().includes(term);
  }).slice(0, pageSize);

  return (
    <div className="panel">
      <PageHeader title="Consultation Queue" subtitle="Checked-in patients waiting for doctor" />
      {loading && <div className="loading-bar" />}
      <TableToolbar
        pageSize={pageSize}
        onPageSizeChange={setPageSize}
        search={search}
        onSearchChange={setSearch}
        searchPlaceholder="Search patient or MR"
        total={queue.length}
      />
      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr><th>Patient</th><th>MRNo</th><th>Type</th><th>Doctor</th><th>Status</th><th>Action</th></tr>
          </thead>
          <tbody>
            {filtered.map(q => (
              <tr key={q.id}>
                <td>{q.patientName}</td>
                <td>{q.mrNumber}</td>
                <td>{q.patientType}</td>
                <td>{q.doctorName ?? '—'}</td>
                <td><span className="soon-badge" style={{ background: '#dcfce7', color: '#166534' }}>{q.status}</span></td>
                <td><button type="button" className="btn small primary" onClick={() => openConsult(q)}>Consult</button></td>
              </tr>
            ))}
            {filtered.length === 0 && !loading && (
              <tr><td colSpan={6} className="empty">No checked-in patients. Check in from Patient Vault first.</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
