# ParkMate — Smart Parking Management System

ParkMate is a web-based Smart Parking Management System developed for the BCA (Honours) Full Stack Web Development Project using ASP.NET Core.

## Technology stack

- ASP.NET Core MVC / C# / Razor Views (.NET 10)
- Entity Framework Core 10
- Microsoft SQL Server
- HTML, CSS, JavaScript
- REST API controllers
- Session-based login state
- Brevo SMTP for email OTP (requires private local configuration)

## Main modules

- Registration, login, OTP verification, forgot/reset/set password
- User dashboard, profile, booking history, feedback and payment history
- Booking duration and vehicle details, slot selection, booking confirmation and payment-result flow
- Receipt PDF and refund receipt PDF
- Admin dashboard and management of bookings, slots, pricing, feedback, payments, refunds and user records
- REST APIs for slots, pricing, bookings and payments
- Global exception-handling middleware
- Responsive CSS layer for smaller screens

> Payment is a project/demo workflow. This repository does not integrate a live payment gateway. Physical gate hardware, RFID, cameras and IoT sensors are outside the current scope. The app has not been deployed to a public URL.

## Requirements

- Visual Studio 2026 or another compatible IDE with the .NET 10 SDK
- SQL Server and SQL Server Management Studio
- A database named `ParkMateDb`

## Run locally

1. Clone or download this repository.
2. Open `ParkMate.slnx` (or `ParkMate.csproj`) in Visual Studio.
3. In `appsettings.json`, update `ConnectionStrings:DefaultConnection` for your local SQL Server instance. The committed value is only an example for a local SQL Server instance named `PARKMATE`.
4. Create an empty database named `ParkMateDb`, or ensure your SQL Server account can create it.
5. In Visual Studio, open **Tools → NuGet Package Manager → Package Manager Console**, set **Default project** to `ParkMate`, and run:

   ```powershell
   Update-Database
   ```

   This applies the EF Core migration and creates the schema. It does not restore your private/local records or create a production admin account.
6. If you need email OTP locally, copy `appsettings.Development.example.json` to `appsettings.Development.json` and enter your own Brevo SMTP values and verified sender email. The real `appsettings.Development.json` is intentionally excluded from Git and must stay local.
7. Start the app using Visual Studio (F5 / Run).

For schema reference and additional details, see `DATABASE_SETUP.md` and `Database/ParkMate_Schema.sql`.

## Database / sample data note

The EF Core migration and `Database/ParkMate_Schema.sql` document the database structure. Personal user records, password hashes, OTP records, payments, bookings and email credentials are not included. A newly created database needs its own role records and any initial slots/pricing/admin setup before all workflows can be used. Do not copy live/private database records into a public repository.

## Repository safety

- Never commit `appsettings.Development.json`, SMTP keys, passwords, database backups or published credentials.
- If an SMTP key was previously included in a shared ZIP, rotate it with the provider before using it again.
- `bin`, `obj` and `.vs` are generated local files; they are excluded via `.gitignore`.

## Academic scope

The project addresses MVC development, forms and validation, SQL Server / EF Core, CRUD/search, REST APIs, authentication, and error handling. Deployment has not been completed because it was not currently required by the project guide; confirm the final requirement with the faculty member.
