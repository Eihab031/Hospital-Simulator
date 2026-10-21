# Domain Model — Hospital Simulation

The vocabulary of the hospital simulation. Everything below describes **what exists** in the world, not how it runs. Services (the clock, the sim engine, the person provider) live outside this document.

Last updated: Phase 0.

## Table of contents

1. Entities
2. Relationships
3. Persistence
4. Design decisions
5. Read models (presentation layer)
6. Glossary

---

## 1. Entities

### Person
Identity of a human being, independent of any hospital stay. The only entity persisted to the database.

- `Id` — Guid. Internal identity.
- `FirstName` — string.
- `LastName` — string.
- `FullName` — computed from FirstName + LastName. Not stored.
- `Gender` — enum { Male, Female }.
- `Email` — string. Used by the Patients tab search.
- `DateOfBirth` — DateTime. Age is derived from this, not stored (would go stale as the simulated clock advances).
- `Age` — computed. Not stored.
- `Address` — string. Single combined string ("street, city, state, country, postcode") built at import time. One field, not five.
- `PhotoUrl` — string? (nullable). Present for API-sourced people; null for locally generated fallback people.

### Patient
One hospital episode. A Patient **is** one admission — from arrival to outcome. Holds a reference to a Person (composition, not inheritance).

- `Id` — Guid.
- `Person` — Person reference. The source of truth for name, email, photo, gender, DOB, address.
- `Sickness` — Sickness reference (from the static catalog).
- `AdmittedAt` — DateTime (simulated time).
- `ExpectedDischargeAt` — DateTime (simulated time). Computed once at admission: `AdmittedAt + daysRolled`. Frozen — does not change.
- `DischargedAt` — DateTime? (nullable). null while active. Set at discharge or death.
- `Bed` — Bed reference. Frozen at admission; never changes. Historical fact.
- `Outcome` — enum { Active, Discharged, Deceased }. Starts as Active.
- `WillDie` — bool. Rolled once at admission using the sickness's DeathChance. Hidden from the UI (otherwise it wouldn't be a simulation).
- `DaysStayed` — computed double. `(DischargedAt ?? SimulatedNow) - AdmittedAt`, in days.

### Sickness
Static catalog entry. Hardcoded in code (SicknessCatalog), not in the database.

- `Kind` — SicknessKind enum (identity).
- `Name` — string. Display label (e.g. "Heart Attack").
- `MinDays` — int. Lower bound of stay length.
- `MaxDays` — int. Upper bound.
- `DeathChance` — double (0.0–1.0). Rolled once at admission.
- `Weight` — int. Used for weighted random selection. Higher weight = more common.

`SicknessKind` values: `Flu, Pneumonia, Surgery, Fracture, HeartAttack, Appendicitis, Stroke, Infection`.

### Room
A physical room. Dumb container.

- `Number` — int (1..50). Human-readable label. Not a Guid — it's a label, not runtime identity.
- `Beds` — List<Bed> (exactly 2).

### Bed
A physical bed.

- `Id` — Guid. For lookups.
- `Number` — int (1 or 2). Label within the room.
- `Room` — Room reference. Back-reference to owning Room.
- `CurrentPatient` — Patient? (nullable). null = free. **The source of truth for occupancy.**
- `IsFree` — computed. `CurrentPatient is null`. Not stored.

### Hospital
The whole session's world. Root object.

- `Rooms` — List<Room> (exactly 50).
- `Patients` — List<Patient>. Every admission this session, active or finished.
- `PersonPool` — Queue<Person>. The 100 seeded people, drawn FIFO by the arrival logic. When empty, arrivals fall back to the local name generator.
- `SimulatedNow` — DateTime. Starts at the real current time at launch. Advanced by the sim tick.
- `Stats` — SimulationStats.

### SimulationStats
The dashboard's numbers. Stored as counters (see "Design decisions").

- `TotalArrivals` — int. Every person who arrived (admitted + transferred).
- `TotalAdmissions` — int. Arrivals that got a bed.
- `TotalTransferred` — int. Arrivals with no free bed (sent to nearest hospital).
- `TotalDischarged` — int.
- `TotalDeceased` — int.
- `PeakOccupancy` — int. Max number of beds occupied at any moment this session.

Chart data (arrivals/discharges per simulated day, current sickness distribution) is **derived on demand** from `Hospital.Patients` — not stored.

---

## 2. Relationships

### Ownership

| Parent owns | Children | Cardinality |
|---|---|---|
| Hospital | Rooms | 1 → exactly 50 |
| Room | Beds | 1 → exactly 2 |
| Hospital | Patients | 1 → 0..N (grows) |
| Hospital | PersonPool | 1 → 0..N (Queue, shrinks) |
| Hospital | Stats | 1 → 1 |
| Hospital | SimulatedNow | 1 → 1 value |

Destroying the parent destroys the children. Rooms cannot exist outside a Hospital; Beds cannot exist outside a Room.

### Cross-references

| Reference | Cardinality | Source of truth for |
|---|---|---|
| Bed.CurrentPatient → Patient | 0 or 1 | "Who is in this bed right now" (nullable; null = free) |
| Patient.Person → Person | many → 1 | Identity of the patient |
| Patient.Bed → Bed | many → 1 | "Which bed was this patient admitted to" (historical; frozen at admission) |
| Patient.Sickness → Sickness | many → 1 | The sickness this admission is for (frozen at admission) |

**Note:** `Bed.CurrentPatient` and `Patient.Bed` are a bidirectional link. They agree during active admission, and disagree after discharge (the bed is free, but the patient's record still points at the bed). This is intentional: one is *current state*, the other is *historical state*.

**Transferred people are not Patients.** They arrived, found no bed, and left for another hospital. Only the counter `TotalTransferred` records them.

---

## 3. Persistence

| Entity | Where | Why |
|---|---|---|
| Person | MariaDB — single table `People`, max 100 rows | The 100 seeded people must survive an API failure. This is the cache. |
| Patient | Memory | Session state. Reset each launch by design. |
| Sickness | Code (static catalog) | Hardcoded config, 8 entries. Never changes at runtime. |
| Room | Memory | Session state. |
| Bed | Memory | Session state. |
| Hospital | Memory | The session root. |
| SimulationStats | Memory | Session counters. |
| PatientRow / RoomCard / DashboardSnapshot | Not stored | Read models (see §5). |

**Rule for future features:** ask "does this state need to survive a restart?" If yes → MariaDB. If no → memory. Today only `People` passes the test.

---

## 4. Design decisions

These are the "why" answers behind the model. Keep them here so future-you doesn't undo them accidentally.

### Composition over inheritance: Patient holds a Person, does not inherit from Person

A Patient **is not a kind of Person** — it's a Person **who has** a hospital episode. Inheritance (`class Patient : Person`) would:

- Duplicate Person fields into the Patient, creating two sources of truth for name/email/etc.
- Fail the lifecycle check (Maria is a Person *before* admission and *after* discharge — she's a Patient *only during* the stay).
- Break Liskov substitution: a `Patient` would carry admission invariants (`Bed != null while Active`) that a `Person` type wouldn't guarantee.

Composition (`Patient.Person = maria`) makes Maria one object, referenced from multiple places. One source of truth. Correct lifecycle. No hidden invariants.

Same reasoning applies to `Patient.Sickness` and `Patient.Bed` — all composition, all references.

### Guid for runtime identity, enum for closed choices, int for labels

- **Guid** — for things created at runtime whose identity must persist across references: `Person.Id`, `Patient.Id`, `Bed.Id`. (Sessions reset every launch, so a `Person.Id` of 1 would mean 100 different people over 100 launches. Guid avoids that.)
- **Enum** — for closed, hardcoded sets of choices: `Gender`, `Outcome`, `SicknessKind`. Compile-time safety; exhaustive switches; no typo bugs.
- **int** — for human-facing labels where a number is what the user reads: `Room.Number` (1..50), `Bed.Number` (1..2).
- **Never** — string for identity (typo-prone, no compile-time safety) or string for closed choices (same reason). We use string only where it's genuinely free text: `Person.Email`, `Person.Address`, `Sickness.Name` (display only).

### Derived vs stored

**Store facts, derive views.** 

- We store `DateOfBirth` and compute `Age`. (Storing Age would go stale as the simulated clock moves.)
- We store `AdmittedAt` and `ExpectedDischargeAt` and compute `DaysStayed`.
- We store `Bed.CurrentPatient` and compute `Bed.IsFree`.

**Exception (deliberate for learning):** `SimulationStats` stores explicit counters even though most are derivable from `Hospital.Patients`. Rationale: for 100-ish records, deriving is trivial, and explicit counters make the simulation's behavior *visible and debuggable*. Revisit as a refactor exercise once everything works.

### Patient = one admission (not Patient + Stay)

A Patient record **is** one hospital episode. It's created at admission and reaches an Outcome (Discharged or Deceased). After that, it stays in memory for the session (so the Patients tab can list it) but is never reopened. The same human arriving twice would be two separate Patient records — this scenario doesn't occur in the current simulation (the name generator produces fresh Persons), so modeling it would be over-engineering.

If returning patients are ever added as a feature, the refactor is: split `Patient` into `Patient` (identity + persistent status) and `Stay` (one per admission). This is a meaningful refactor, but the code's layering makes it tractable.

### Sickness is static config, not a database table

The 8 sicknesses are hardcoded. They do not change at runtime. Putting them in the DB would mean: a migration, a seeding step, a query, and a cache layer — all to store 8 rows that we typed by hand. The cost outweighs the benefit. If the catalog ever needs to be user-editable, that's the moment it moves to the DB.

---

## 5. Read models (presentation layer)

These are **not** domain entities. They're flat objects built on demand in the UI layer to feed specific screens. The domain never sees them.

### PatientRow
One row of the Patients table.

`PatientId, FullName, Email, Gender, Age, SicknessName, BedLabel, AdmittedAt, DischargedAt, Outcome, DaysStayed, PhotoUrl?`

Built from: `Patient` + its `Person` + its `Bed` + its `Bed.Room`.

### RoomCard
One card in the Simulation grid.

`RoomNumber, Bed1: BedCard, Bed2: BedCard`

where `BedCard = { Label, IsFree, OccupantName?, SicknessName?, NextFreeAt? }`.

### DashboardSnapshot
The dashboard's current numbers.

`OccupancyPercent, FreeBeds, TotalArrivals, TotalTransfers, TotalDischarges, TotalDeaths, SicknessDistribution, DailyHistory`

Built from: `Hospital` + `SimulationStats` + LINQ over `Hospital.Patients`.

---

## 6. Glossary

Quick reference for terms used across the codebase.

- **Person** — a human being. Identity only. Persisted.
- **Patient** — a Person with a hospital episode. One admission. Session-only.
- **Admission** — the act of giving a patient a bed. Synonymous with "the patient's stay."
- **Stay** — informal synonym for "the patient's admission." (We do **not** have a separate `Stay` entity — see §4.)
- **Outcome** — how an admission ended: Discharged, Deceased, or (while ongoing) Active.
- **Discharge** — a patient finishing their stay and leaving alive.
- **Death** — a patient's `WillDie` roll succeeding; outcome becomes Deceased at end of stay.
- **Transfer** — an arrival that found no free bed and left for another hospital. Not a Patient; only counted.
- **Occupancy** — the number of beds whose `CurrentPatient` is not null.
- **Free bed** — a bed whose `CurrentPatient` is null.
- **Simulated time** — the clock that advances at 12 hospital hours per real second (scaled by the speed multiplier). Not the system clock.
- **Hospital day** — 24 hours of simulated time (= 2 real seconds at ×1 speed).