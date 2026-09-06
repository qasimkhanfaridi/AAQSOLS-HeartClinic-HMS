import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

type MenuItem = {
  to?: string;
  label: string;
  roles: string[];
  soon?: boolean;
};

type MenuGroup = {
  label: string;
  items: MenuItem[];
};

const menu: MenuGroup[] = [
  {
    label: 'Patient Management',
    items: [
      { to: '/patients/add', label: 'Add Patient', roles: ['ADMIN', 'RECEPTION'] },
      { to: '/patients/vault', label: "Patient's Vault", roles: ['ADMIN', 'RECEPTION', 'DOCTOR'] },
      { to: '/coming-soon?feature=Patient Monitory', label: 'Patient Monitory', roles: ['ADMIN', 'RECEPTION'], soon: true },
      { to: '/coming-soon?feature=Checked In Status', label: 'Checked In Status', roles: ['ADMIN', 'RECEPTION'], soon: true },
      { to: '/coming-soon?feature=Diagnostics Reports', label: 'Diagnostics/ Investigations Reports', roles: ['ADMIN', 'RECEPTION', 'DOCTOR'], soon: true },
      { to: '/coming-soon?feature=EMR Search', label: 'EMR Search', roles: ['ADMIN', 'DOCTOR'], soon: true },
      { to: '/coming-soon?feature=Challan Vault', label: 'Challan Vault', roles: ['ADMIN', 'RECEPTION'], soon: true },
      { to: '/coming-soon?feature=Update Doctor Checkin Patient Info', label: 'Update Doctor Checkin Patient Info', roles: ['ADMIN', 'RECEPTION'], soon: true },
    ],
  },
  {
    label: 'Doctor Management',
    items: [
      { to: '/consultation', label: 'Consultation Queue', roles: ['ADMIN', 'DOCTOR'] },
      { to: '/coming-soon?feature=Doctor Schedule', label: 'Doctor Schedule', roles: ['ADMIN'], soon: true },
    ],
  },
  {
    label: 'Imaging And Others',
    items: [{ to: '/imaging/orders', label: 'Imaging Orders', roles: ['ADMIN', 'DOCTOR'] }],
  },
  {
    label: 'Lab Investigations',
    items: [
      { to: '/lab/pending-payments', label: 'Lab Orders', roles: ['ADMIN', 'RECEPTION'] },
      { to: '/lab/pending-payments', label: 'Pending Payments', roles: ['ADMIN', 'RECEPTION'] },
    ],
  },
  {
    label: 'Medicine',
    items: [
      { to: '/medicines', label: 'Medicine', roles: ['ADMIN', 'DOCTOR'] },
      { to: '/coming-soon?feature=Medicine Category', label: 'Category', roles: ['ADMIN'], soon: true },
      { to: '/coming-soon?feature=Medicine Generic', label: 'Generic', roles: ['ADMIN'], soon: true },
      { to: '/coming-soon?feature=Medicine Type', label: 'Type', roles: ['ADMIN'], soon: true },
      { to: '/coming-soon?feature=Medicine Strength', label: 'Strength', roles: ['ADMIN'], soon: true },
      { to: '/coming-soon?feature=Medicine Dosage', label: 'Dosage', roles: ['ADMIN'], soon: true },
      { to: '/coming-soon?feature=Medicine Route', label: 'Route', roles: ['ADMIN'], soon: true },
      { to: '/coming-soon?feature=Disposable', label: 'Disposable', roles: ['ADMIN'], soon: true },
    ],
  },
  {
    label: 'Inventory',
    items: [{ to: '/coming-soon?feature=Stock Ledger', label: 'Stock Ledger', roles: ['ADMIN'], soon: true }],
  },
  {
    label: 'Statistics',
    items: [
      { to: '/reports/staff-performance', label: 'Staff Performance Details', roles: ['ADMIN'] },
      { to: '/reports/pharmacy-census', label: 'Pharmacy Census Report', roles: ['ADMIN'] },
      { to: '/reports/services-cash-flow', label: 'Services Cash Flow', roles: ['ADMIN'] },
      { to: '/reports/reception-cash-flow', label: 'Reception Cash Flow', roles: ['ADMIN'] },
      { to: '/reports/lab-cash-flow', label: 'Lab Cash Flow', roles: ['ADMIN'] },
      { to: '/reports/region-wise', label: 'Region Wise Report', roles: ['ADMIN'] },
      { to: '/reports/average-opd', label: 'Average OPD Stats', roles: ['ADMIN'] },
    ],
  },
];

export function Layout() {
  const { displayName, roleCode, logout } = useAuth();
  const initial = displayName.charAt(0).toUpperCase() || 'U';

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-icon">♥</div>
          <div>
            <strong>AAQSOLS</strong>
            <span>PulseCore HMS</span>
          </div>
        </div>
        <nav className="sidebar-nav">
          {menu.map(group => {
            const visible = group.items.filter(i => i.roles.includes(roleCode));
            if (visible.length === 0) return null;
            return (
              <div key={group.label} className="menu-group">
                <div className="menu-title">{group.label}</div>
                {visible.map(item => (
                  <NavLink
                    key={item.label}
                    to={item.to!}
                    className={({ isActive }) =>
                      item.soon
                        ? 'menu-link soon'
                        : isActive
                          ? 'menu-link active'
                          : 'menu-link'
                    }
                  >
                    <span>{item.label}</span>
                    {item.soon && <span className="soon-badge">Soon</span>}
                  </NavLink>
                ))}
              </div>
            );
          })}
        </nav>
        <div className="sidebar-footer">© AAQSOLS PulseCore</div>
      </aside>
      <div className="main-area">
        <header className="topbar">
          <div className="topbar-left">
            <div className="clinic-logo">HC</div>
            <h1>The Heart Clinic</h1>
          </div>
          <div className="user-chip">
            <span className="user-name">{displayName}</span>
            <div className="avatar">{initial}</div>
            <button type="button" className="link-btn" onClick={logout}>Logout</button>
          </div>
        </header>
        <main className="content"><Outlet /></main>
      </div>
    </div>
  );
}
