# AAQSOLS Heart Clinic HMS

**PulseCore PKG-A replica** — LOOM demo rebuild with owned source code.

## Architecture

| Layer | Tech |
|-------|------|
| Frontend | React 18 + TypeScript + Vite |
| Backend | .NET 10 Web API + JWT |
| Database | SQL Server (LocalDB dev) — **normalized, LOOM migration-ready** |

## Quick start

### Option A — scripts (recommended)

**Terminal 1 — API**

```cmd
scripts\start-api.cmd
```

**Terminal 2 — Web**

```cmd
cd src\Web
npm.cmd run dev
```

- API: http://localhost:5080  
- Web: http://localhost:5173  

### Option B — manual

```powershell
cd src\Api
dotnet run
```

```powershell
cd src\Web
npm.cmd run dev
```

> **PowerShell blocks `npm`?** Use `npm.cmd` instead.

## Demo logins

| User | Password | Role |
|------|----------|------|
| admin | Admin@123 | Administrator |
| reception | Reception@123 | Reception |
| doctor | Doctor@123 | Doctor |

## Implemented flows (LOOM replica)

| Phase | Features |
|-------|----------|
| MVP | Login, Staff Performance, Add Patient, Patient Vault, Check-in + challan, Consultation, Medicine master |
| Phase 2 | Panel billing, Lab payment queue, Lab/Services/Reception Cash Flow, Pharmacy Census |
| Phase 3 | Multi-branch vault, Region Wise report, Average OPD stats, Imaging orders |

## LOOM data migration

See [docs/LOOM-Migration-Strategy.md](docs/LOOM-Migration-Strategy.md)

Schema includes `LegacySourceSystem`, `LegacyExternalId`, `ImportBatch` on core entities — ready when you export data from `192.168.1.250`.

## Troubleshooting

**Build fails — file locked by `HeartClinicHms.Api.exe`**

```cmd
taskkill /F /IM HeartClinicHms.Api.exe
```

Or use `scripts\start-api.cmd` (builds to `run\api7\`).

**Reset database**

```sql
DROP DATABASE HeartClinicHms;
```

Restart API — migrations and seed data run automatically.

## Related repos

- Planning & docs: [Modular-HMS-Platform](https://github.com/qasimkhanfaridi/Modular-HMS-Platform)
