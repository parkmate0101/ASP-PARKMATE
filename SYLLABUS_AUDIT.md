# ParkMate source vs course syllabus — quick audit

This audit is based on the uploaded `ParkMate.zip` contents. It verifies file/source presence, not a fresh runtime test on the student's computer.

| Syllabus area | Evidence found in the uploaded project | Status / note |
| --- | --- | --- |
| Phase I: ASP.NET Core MVC setup, controllers, Razor views, routing, layout | `ParkMate.csproj` targets `net10.0`; `Program.cs`; `Controllers/`; `Views/`; three shared layouts; static CSS/JS | Present |
| Phase II: registration, forms, validation, dashboard, navigation | `AuthController`, `OtpController`, login/register/OTP/password views, view models, user/admin dashboards, validation scripts | Present in source; runtime testing is the student's local evidence |
| Phase III: SQL Server, EF Core, migrations, CRUD/search/testing | `ParkMateDbContext`, `Migrations/20260922105332_InitialParkMateDb.cs`, CRUD controllers for slots/pricing, Users search; `ID_NOTE.sql` is a diagnostic query | Core code present. A real full database backup/data export is not included; migration/schema creates structure only |
| Phase IV: REST API, integration, authentication, error handling | `Controllers/Api/` with bookings/payments/pricing/slots; `app.MapControllers()`; auth/session/filters; `GlobalExceptionMiddleware`; JS API integration | Present in source |
| Deployment | No hosted URL or publish profile in this ZIP | Not completed. User said faculty indicated it may not be mandatory; confirm with faculty. Syllabus text itself lists deployment and a deployed link as a deliverable |
| Final report (15–25 pages) | Uploaded project contains older `ParkMate-WORD.docx` / PDF copies | Do not use these old embedded files as the source repository's report. Submit the separate updated ASP.NET report; check it is 15–25 pages and uses the course's index template |
| PPT | Uploaded project contains `ParkMate_Presentation-PDF.pdf`, but no editable `.pptx` inside this ZIP | Keep the separate updated `.pptx` outside the source folder and submit it separately |
| GitHub/source | No `.gitignore` or README in the uploaded ZIP; `.vs`, `bin`, `obj`, `.csproj.user`, and development settings are present | Do not upload the original ZIP as-is. Use the prepared GitHub-ready source package instead |

## Critical safety finding

`appsettings.Development.json` in the uploaded ZIP contains non-placeholder SMTP configuration values. It is excluded from the GitHub-ready package. Do not publish it. Rotate the SMTP key if the real key was shared in a backup and is still active.

## Files excluded from the GitHub-ready package

- `.vs/`, `bin/`, `obj/`
- `appsettings.Development.json`
- `ParkMate.csproj.user`
- old embedded report/PDF/presentation exports

The GitHub-ready package adds `README.md`, `.gitignore`, `appsettings.Development.example.json`, `DATABASE_SETUP.md`, `Database/ParkMate_Schema.sql`, `FILE_USAGE_MAP.md`, and this guide/audit.
