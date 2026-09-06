import { type FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiSafe, type LoginResponse } from '../api/client';
import { useAuth } from '../context/AuthContext';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [userName, setUserName] = useState('admin');
  const [password, setPassword] = useState('Admin@123');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const data = await apiSafe<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ userName, password }),
      });
      login(data);
      navigate(data.roleCode === 'DOCTOR' ? '/consultation' : '/reports/staff-performance');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      <form className="login-card" onSubmit={onSubmit}>
        <div className="brand large">
          <div className="brand-icon">♥</div>
          <div>
            <strong>The Heart Clinic</strong>
            <span>PulseCore HMS by AAQSOLS</span>
          </div>
        </div>
        <h2>Sign in to your account</h2>
        <label>Username
          <input value={userName} onChange={e => setUserName(e.target.value)} required autoComplete="username" />
        </label>
        <label>Password
          <input type="password" value={password} onChange={e => setPassword(e.target.value)} required autoComplete="current-password" />
        </label>
        {error && <div className="flash error">{error}</div>}
        <button type="submit" className="btn primary" style={{ width: '100%', marginTop: '.5rem' }} disabled={loading}>
          {loading ? 'Signing in...' : 'Login'}
        </button>
        <p className="hint">
          Demo credentials — Admin: <strong>admin</strong> / Admin@123 · Reception: <strong>reception</strong> / Reception@123 · Doctor: <strong>doctor</strong> / Doctor@123
        </p>
      </form>
    </div>
  );
}
