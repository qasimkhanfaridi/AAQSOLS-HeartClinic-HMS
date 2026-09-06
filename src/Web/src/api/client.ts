// Empty = same origin; Vite proxies /api → backend (see vite.config.ts)
const API_BASE = import.meta.env.VITE_API_URL ?? '';

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('token');
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
  if (res.status === 401) {
    localStorage.removeItem('token');
    window.location.href = '/login';
    throw new Error('Unauthorized');
  }
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: res.statusText }));
    throw new Error(err.message ?? `Request failed (${res.status})`);
  }
  return res.json();
}

export async function apiSafe<T>(path: string, options: RequestInit = {}): Promise<T> {
  try {
    return await api<T>(path, options);
  } catch (e) {
    if (e instanceof TypeError) {
      throw new Error('Cannot reach API. Start the backend: cd src\\Api && dotnet run (port 5080)');
    }
    throw e;
  }
}

export interface LoginResponse {
  token: string;
  displayName: string;
  roleCode: string;
  roleName: string;
}

export interface Patient {
  id: string;
  firstName: string;
  guardianName: string;
  mrNumber: string;
  cnicOrPassport: string;
  gender: string;
  patientType: string;
  mobile?: string;
  registeredOn: string;
  corporatePanelId?: string;
  corporatePanelName?: string;
  branchName?: string;
  branchRegion?: string;
}

export interface Doctor {
  id: string;
  fullName: string;
  specialty: string;
}

export interface ServiceGroup {
  id: string;
  name: string;
  services: { id: string; name: string; charges: number; deliveryDays?: number }[];
}

export interface ActiveCheckIn {
  id: string;
  patientId: string;
  patientName: string;
  mrNumber: string;
  patientType: string;
  doctorName?: string;
  status: string;
  checkedInAt: string;
}

export interface Consultation {
  id: string;
  checkInId: string;
  patientName: string;
  status: string;
  referToCategory: boolean;
  entries: { id: string; sectionType: string; content: string; recordedAt: string }[];
  prescriptions: { id: string; medicineName: string; dosage?: string; strength?: string }[];
}

export interface Medicine {
  id: string;
  name: string;
  category: string;
  generic: string;
  dosage?: string;
  route?: string;
  strength?: string;
  stock: number;
}

export interface StaffPerformanceReport {
  from: string;
  to: string;
  rows: {
    userName: string;
    registered: number;
    opd: number;
    er: number;
    ipd: number;
    services: number;
    doctor: number;
    visitedTotal: number;
  }[];
}

export interface FilterOption {
  id: string;
  name: string;
  code?: string;
}

export interface ReportFilters {
  services: FilterOption[];
  doctors: FilterOption[];
  departments: FilterOption[];
  patientTypes: FilterOption[];
  stores: FilterOption[];
}

export interface LabCashFlowDetailRow {
  date: string;
  patientName: string;
  mrNumber: string;
  serviceName: string;
  doctorName?: string;
  patientType: string;
  charges: number;
}

export interface LabCashFlowSummaryRow {
  serviceName: string;
  count: number;
  totalCharges: number;
}

export interface LabCashFlowReport {
  from: string;
  to: string;
  reportType: string;
  detailedRows?: LabCashFlowDetailRow[];
  summaryRows?: LabCashFlowSummaryRow[];
  grandTotal: number;
}

export interface PharmacyCensusRow {
  date: string;
  males: number;
  females: number;
  total: number;
}

export interface PharmacyCensusReport {
  from: string;
  to: string;
  rows: PharmacyCensusRow[];
}

export interface LabOrder {
  id: string;
  patientName: string;
  mrNumber: string;
  serviceName: string;
  charges: number;
  status: string;
  orderedAt: string;
}

export interface ServicesCashFlowReport {
  from: string;
  to: string;
  rows: { date: string; serviceName: string; patientName: string; patientType: string; charges: number }[];
  grandTotal: number;
}

export interface ReceptionCashFlowReport {
  from: string;
  to: string;
  rows: {
    date: string;
    collectedBy: string;
    patientName: string;
    challanNumber: string;
    patientCollected: number;
    panelAmount: number;
    subTotal: number;
  }[];
  totalPatientCollected: number;
  totalPanelAmount: number;
}

export interface ImagingOrder {
  id: string;
  patientName: string;
  mrNumber: string;
  serviceName: string;
  charges: number;
  deliveryDate?: string;
  status: string;
  orderedAt: string;
}

export interface RegionWiseReport {
  from: string;
  to: string;
  rows: { region: string; branchName: string; registered: number; opd: number; revenue: number }[];
}

export interface AverageOpdReport {
  from: string;
  to: string;
  dailyRows: { date: string; opdCount: number }[];
  totalOpd: number;
  averageOpdPerDay: number;
  peakDayCount: number;
  peakDate?: string;
}

export interface ChallanRecord {
  id: string;
  challanNumber: string;
  patientName: string;
  mrNumber: string;
  services: string;
  subTotal: number;
  patientPayable: number;
  panelPayable: number;
  issuedAt: string;
  issuedBy: string;
}
