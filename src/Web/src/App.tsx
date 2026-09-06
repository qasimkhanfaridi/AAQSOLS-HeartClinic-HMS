import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { StaffPerformancePage } from './pages/StaffPerformancePage';
import { AddPatientPage } from './pages/AddPatientPage';
import { PatientVaultPage } from './pages/PatientVaultPage';
import { ConsultationPage } from './pages/ConsultationPage';
import { MedicineListPage } from './pages/MedicineListPage';
import { LabCashFlowPage } from './pages/LabCashFlowPage';
import { PharmacyCensusPage } from './pages/PharmacyCensusPage';
import { LabPendingPaymentsPage } from './pages/LabPendingPaymentsPage';
import { ServicesCashFlowPage } from './pages/ServicesCashFlowPage';
import { ReceptionCashFlowPage } from './pages/ReceptionCashFlowPage';
import { AverageOpdPage } from './pages/AverageOpdPage';
import { RegionWisePage } from './pages/RegionWisePage';
import { ImagingOrdersPage } from './pages/ImagingOrdersPage';
import { ChallanVaultPage } from './pages/ChallanVaultPage';
import { ComingSoonPage } from './pages/ComingSoonPage';
import './index.css';

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const { token } = useAuth();
  if (!token) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/" element={<PrivateRoute><Layout /></PrivateRoute>}>
          <Route index element={<Navigate to="/reports/staff-performance" replace />} />
          <Route path="reports/staff-performance" element={<StaffPerformancePage />} />
          <Route path="reports/lab-cash-flow" element={<LabCashFlowPage />} />
          <Route path="reports/pharmacy-census" element={<PharmacyCensusPage />} />
          <Route path="reports/services-cash-flow" element={<ServicesCashFlowPage />} />
          <Route path="reports/reception-cash-flow" element={<ReceptionCashFlowPage />} />
          <Route path="reports/region-wise" element={<RegionWisePage />} />
          <Route path="reports/average-opd" element={<AverageOpdPage />} />
          <Route path="imaging/orders" element={<ImagingOrdersPage />} />
          <Route path="lab/pending-payments" element={<LabPendingPaymentsPage />} />
          <Route path="coming-soon" element={<ComingSoonPage />} />
          <Route path="patients/add" element={<AddPatientPage />} />
          <Route path="patients/vault" element={<PatientVaultPage />} />
          <Route path="challans/vault" element={<ChallanVaultPage />} />
          <Route path="consultation" element={<ConsultationPage />} />
          <Route path="medicines" element={<MedicineListPage />} />
        </Route>
      </Routes>
    </AuthProvider>
  );
}
