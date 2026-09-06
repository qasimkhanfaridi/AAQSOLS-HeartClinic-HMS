import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import type { LoginResponse } from '../api/client';

interface AuthState {
  token: string | null;
  displayName: string;
  roleCode: string;
  login: (data: LoginResponse) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [displayName, setDisplayName] = useState(localStorage.getItem('displayName') ?? '');
  const [roleCode, setRoleCode] = useState(localStorage.getItem('roleCode') ?? '');

  const value = useMemo<AuthState>(() => ({
    token,
    displayName,
    roleCode,
    login: (data) => {
      localStorage.setItem('token', data.token);
      localStorage.setItem('displayName', data.displayName);
      localStorage.setItem('roleCode', data.roleCode);
      setToken(data.token);
      setDisplayName(data.displayName);
      setRoleCode(data.roleCode);
    },
    logout: () => {
      localStorage.clear();
      setToken(null);
      setDisplayName('');
      setRoleCode('');
    },
  }), [token, displayName, roleCode]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
