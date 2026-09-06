import { type FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import { PageHeader } from '../components/PageHeader';

export function AddPatientPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    firstName: '', guardianName: '', gender: 'Male', cnicOrPassport: '',
    patientTypeCode: 'PRIVATE', mobile: '', address: '', dateOfBirth: '',
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [saving, setSaving] = useState(false);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setSuccess('');
    setSaving(true);
    try {
      const result = await api<{ id: string; mrNumber: string }>('/api/patients', {
        method: 'POST',
        body: JSON.stringify({
          firstName: form.firstName,
          guardianName: form.guardianName,
          gender: form.gender,
          cnicOrPassport: form.cnicOrPassport,
          patientTypeCode: form.patientTypeCode,
          mobile: form.mobile || null,
          address: form.address || null,
        }),
      });
      setSuccess(`Patient saved successfully. MR Number: ${result.mrNumber}`);
      setTimeout(() => navigate('/patients/vault'), 1500);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Save failed');
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="panel">
      <PageHeader
        title="Add Patient"
        subtitle="One CNIC / Passport per patient record"
        actions={<Link to="/patients/vault" className="btn outline" style={{ textDecoration: 'none' }}>Back to Vault</Link>}
      />
      <form className="form-grid" onSubmit={onSubmit}>
        <label>First Name *
          <input required value={form.firstName} onChange={e => setForm({ ...form, firstName: e.target.value })} />
        </label>
        <label>Guardian Name *
          <input required value={form.guardianName} onChange={e => setForm({ ...form, guardianName: e.target.value })} />
        </label>
        <label>Gender *
          <select value={form.gender} onChange={e => setForm({ ...form, gender: e.target.value })}>
            <option>Male</option><option>Female</option>
          </select>
        </label>
        <label>CNIC / Passport *
          <input required value={form.cnicOrPassport} onChange={e => setForm({ ...form, cnicOrPassport: e.target.value })} placeholder="37405-9227187-1" />
        </label>
        <label>Patient Type *
          <select value={form.patientTypeCode} onChange={e => setForm({ ...form, patientTypeCode: e.target.value })}>
            <option value="PRIVATE">Private</option>
            <option value="PANEL">Panel</option>
          </select>
        </label>
        <label>Mobile
          <input value={form.mobile} onChange={e => setForm({ ...form, mobile: e.target.value })} />
        </label>
        <label>Date of Birth
          <input type="date" value={form.dateOfBirth} onChange={e => setForm({ ...form, dateOfBirth: e.target.value })} />
        </label>
        <label className="full">Address
          <input value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} />
        </label>
        {error && <div className="flash error full">{error}</div>}
        {success && <div className="flash success full">{success}</div>}
        <div className="actions full">
          <button type="submit" className="btn primary" disabled={saving}>{saving ? 'Saving...' : 'Save Patient'}</button>
          <button type="button" className="btn outline" onClick={() => navigate('/patients/vault')}>Cancel</button>
        </div>
      </form>
    </div>
  );
}
