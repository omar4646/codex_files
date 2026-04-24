# MediBook Desktop

MediBook Desktop is a local Windows desktop appointment booking system for a small clinic. Patients can register, sign in, browse doctors, book open appointment slots, cancel appointments, and reschedule upcoming bookings. Admin/clinic staff can manage doctors, availability, appointments, and specialties.

## Tech stack

- C# / .NET 8
- WPF
- SQLite local database
- Entity Framework Core
- MVVM architecture
- No paid third-party libraries
- Offline-first local operation after NuGet packages are restored

## How to run

1. Install the .NET 8 SDK for Windows.
2. Open PowerShell in this folder:
   `C:\Users\Alpha\OneDrive - International Campus, Zhejiang University\Apps\MediBookDesktop`
3. Restore and build:
   `dotnet restore`
   `dotnet build`
4. Launch:
   `dotnet run --project .\MediBookDesktop\MediBookDesktop.csproj`

This workspace also includes a local SDK under `.dotnet` because the global `dotnet` command was not available during implementation. If you want to use it from this OneDrive path, map the folder to a short drive letter first so MSBuild does not trip over the comma in the parent folder name:

`subst M: "C:\Users\Alpha\OneDrive - International Campus, Zhejiang University\Apps\MediBookDesktop"`

Then run:

`M:\.dotnet\dotnet.exe build M:\MediBookDesktop.sln`

`M:\.dotnet\dotnet.exe run --project M:\MediBookDesktop\MediBookDesktop.csproj`

The database is created automatically on first launch at:
`%LOCALAPPDATA%\MediBook Desktop\medibook.db`

## Default accounts

Admin:

- Email: `admin@medibook.local`
- Password: `Admin123!`

Sample patients:

- `maya@example.local` / `Patient123!`
- `noah@example.local` / `Patient123!`

## Seed data

On first launch the app seeds:

- 1 admin account
- 2 sample patient accounts
- 8 doctors
- 6 specialties
- 3 clinic locations
- Two weeks of weekday appointment slots

## Main features

- Local login and patient registration
- PBKDF2 password hashing with per-password salts
- Patient dashboard with upcoming and past appointments
- Doctor search by name, specialty, location, and available date
- Doctor profile and appointment booking
- Booking confirmation with appointment ID
- Patient appointment cancellation and rescheduling
- Admin dashboard statistics
- Doctor add/edit/delete with future appointment protection
- Availability creation and unavailable-slot marking
- Appointment filtering and status updates
- Specialty add/edit/delete when not assigned to doctors

## Database notes

The app uses `EnsureCreated` instead of migrations to keep the first local version simple. For production-style schema evolution, replace this with EF Core migrations and a formal upgrade path.

## Tests

The test project covers:

- Double-book prevention
- Cancellation freeing the slot
- Past-slot booking rejection
- Password hashing and verification
- Admin doctor creation

Run tests with:

`dotnet test`

## Known limitations

- Authentication is local only and intentionally simple.
- No real payment handling is implemented.
- Reason-for-visit is a short free-text note, not a medical record system.
- SQLite concurrency is suitable for a local desktop app, not a multi-clinic shared deployment.
- Password hashing is secure for a local sample app, but production systems should add account lockout, audit logs, encrypted backups, and stronger identity management.

## Future improvements

- EF Core migrations and versioned database upgrades
- Calendar-style schedule view
- Export appointments to CSV
- Patient profile editing
- Reminder notifications
- Role permissions beyond the two broad roles
