# LOOM Data Migration Strategy

**Product:** AAQSOLS Heart Clinic HMS  
**Source system:** LOOM @ `192.168.1.250:56`  
**Approach:** Schema-first in PulseCore → ETL import when LOOM data export is available

---

## Build order (what we did)

```
1. Normalized database schema (migration-ready)
2. Thin REST API + seed data
3. Working frontend (LOOM demo flows)
4. LOOM data import (when you get export from server)
```

**Frontend first alone would require rework.** **Backend schema first** ensures LOOM patients/medicines map cleanly when data arrives.

---

## Migration-ready design

Every major entity inherits `MigratableEntity`:

| Field | Purpose |
|-------|---------|
| `LegacySourceSystem` | e.g. `"LOOM"` |
| `LegacyExternalId` | Original LOOM primary key (string — GUID or int) |
| `ImportBatchId` | Links to import run |
| `ImportedAtUtc` | When record was migrated |
| `IsLegacyRecord` | True if imported vs created fresh |

**Import tracking tables:** `ImportBatches`, `ImportMappingLogs` (per-row success/failure + raw JSON)

---

## Expected LOOM export (when available from server)

| LOOM data | Maps to our table | Notes |
|-----------|-------------------|-------|
| Patients / MR cards | `Patients` | MR number → `LegacyMrNumber`, CNIC unique |
| Doctors | `Doctors` | Name + specialty |
| Services / departments | `ServiceGroups`, `CatalogServices` | Holter, ECG, etc. |
| Medicines | `Medicines` + lookup tables | Category, generic, route |
| Check-ins / challans | `CheckIns`, `Challans`, `CheckInServiceLines` | Historical billing |
| Consultations / EMR | `Consultations`, `ConsultationEntries` | Section-based |
| Users | `Users`, `Roles` | Re-hash passwords or force reset |

---

## Import process (Phase M1.5 — after LOOM export)

1. Export from LOOM server (SQL backup, CSV, or API if vendor provides)
2. Run `ImportBatch` with entity type = `Patients`
3. ETL script maps columns → our schema, sets `LegacyExternalId`
4. Validate: CNIC counts, MR number ranges, duplicate check
5. Parallel run: LOOM + PulseCore until cutover
6. Cutover: read-only LOOM, PulseCore primary

---

## What to request from LOOM server PC

- SQL Server backup OR table CSV exports
- Sample row counts per table
- MR number format confirmation (`5601-YY-NNNNNN`)
- User list (passwords cannot migrate — users reset on first login)

---

## Tables designed for scale

Current MVP uses **22+ normalized tables**. Phase 2 adds Lab, Panel, Inventory without breaking schema — new tables link via FKs, not JSON blobs.

*AAQSOLS | Migration strategy v1.0*
