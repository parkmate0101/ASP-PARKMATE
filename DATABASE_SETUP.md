# ParkMate database setup

## Recommended: use the EF Core migration

1. Start your local SQL Server instance.
2. Create an empty database named `ParkMateDb` (or grant your login permission to create it).
3. Update `ConnectionStrings:DefaultConnection` in `appsettings.json` to match your local SQL Server and database.
4. In Visual Studio open **Tools → NuGet Package Manager → Package Manager Console**.
5. Set **Default project** to `ParkMate`, then execute:

   ```powershell
   Update-Database
   ```

This applies `Migrations/20260922105332_InitialParkMateDb.cs` and creates the tables/indexes defined in the EF Core model.

## Schema SQL file

`Database/ParkMate_Schema.sql` is a schema-only reference script matching the initial EF Core migration. Run it only against a **new, empty** database if you specifically need the SQL script. Do not run it after `Update-Database` against the same database, because it would attempt to create tables that already exist.

## Records and roles

The migration creates the schema, not private application records. It does not export real users, passwords/password hashes, OTP data, bookings, payments, refunds, or receipts. A new database needs appropriate role records and initial slot/pricing data before all user/admin workflows can operate. Create a demo/admin account securely in your local development database; never publish its password/hash or personal records.

## Email OTP configuration

Email settings belong in the ignored local file `appsettings.Development.json`, copied from `appsettings.Development.example.json`. Use your own SMTP login, SMTP key, and verified sender email. Never commit actual credential values.
