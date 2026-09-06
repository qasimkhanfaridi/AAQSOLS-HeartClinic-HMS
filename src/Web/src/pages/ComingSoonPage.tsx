import { useSearchParams } from 'react-router-dom';
import { PageHeader } from '../components/PageHeader';

export function ComingSoonPage() {
  const [params] = useSearchParams();
  const feature = params.get('feature') ?? 'This feature';

  return (
    <div className="panel">
      <PageHeader title={feature} subtitle="Planned for Phase 2 of PulseCore Heart Clinic HMS" />
      <div className="coming-soon-body">
        <div className="coming-soon-icon">🚧</div>
        <p>
          <strong>{feature}</strong> is listed in the LOOM system menu but was not part of the MVP demo walkthrough.
          It is on the roadmap after Lab Cash Flow, Pharmacy Census, and panel billing.
        </p>
        <p className="table-meta">
          Current MVP covers: patient registration, check-in, consultation, medicine master, and core statistics reports.
        </p>
      </div>
    </div>
  );
}
