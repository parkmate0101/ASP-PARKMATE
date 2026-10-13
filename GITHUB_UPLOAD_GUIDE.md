# Publish ParkMate to GitHub (Windows + GitHub Desktop)

## Repository form on github.com/new

- **Owner:** your personal GitHub account.
- **Repository name:** `ASP-PARKMATE` (or another course-approved name).
- **Description:** `Smart Parking Management System built with ASP.NET Core MVC, C#, Entity Framework Core and SQL Server for the BCA Honours Full Stack Web Development Project.`
- **Visibility:** Public if your teacher should be able to open the URL without an invitation; otherwise Private and add your teacher as a collaborator.
- **Add README:** OFF, because this source package already contains `README.md`.
- **Add .gitignore:** None, because this source package already contains `.gitignore`.
- **Add license:** None, unless your teacher specifically asks you to choose one.

Click **Create repository**. Leave the new repository empty so it is easy to clone and fill from this prepared source package.

## Install and sign in to GitHub Desktop

1. Go to https://desktop.github.com/ and install GitHub Desktop.
2. Open it and sign in to the GitHub account that owns `ASP-PARKMATE`.
3. Choose **File → Clone repository**.
4. Select `ASP-PARKMATE` under your GitHub repositories.
5. Choose a local path such as `Documents\GitHub\ASP-PARKMATE` and click **Clone**.
6. In GitHub Desktop choose **Repository → Show in Explorer** to open that cloned folder.

## Copy the prepared source files

1. Download and extract `ParkMate_GitHub_Ready.zip` to a temporary folder.
2. Open the extracted folder. Its contents should include `ParkMate.slnx`, `ParkMate.csproj`, `Program.cs`, `Controllers`, `Models`, `Views`, `Migrations`, `wwwroot`, `README.md`, and `.gitignore` at the top level.
3. Copy **the contents** of this extracted folder into the cloned `ASP-PARKMATE` folder. Do not copy an extra enclosing `ParkMate_GitHub_Ready` folder; the solution and README should sit directly at the repository root.
4. If Windows asks about overwriting files, the cloned repository should be empty; if it is not, stop and check before overwriting anything.

## Commit and publish

1. Return to GitHub Desktop. Wait while it scans the changes.
2. Review the changed-file list. Confirm there is **no** `appsettings.Development.json`, `.vs`, `bin`, `obj`, `ParkMate.csproj.user`, or old report/PDF/PPT file.
3. In the Summary box enter: `Initial commit - ParkMate ASP.NET Core MVC project`.
4. Click **Commit to main** (or the default branch shown).
5. Click **Push origin** if prompted.
6. Click **View on GitHub** and verify the repository displays `README.md`, `Program.cs`, `ParkMate.csproj`, `Controllers`, `Models`, `Views`, `Migrations` and `wwwroot`.

## Never upload these items

- `appsettings.Development.json` (may contain real Brevo SMTP values)
- SMTP passwords/keys, database passwords, API keys, or real password hashes
- `.vs`, `bin`, `obj`, `*.user`, `*.suo`
- local database files/backups (`*.mdf`, `*.ldf`, `*.bak`, `*.bacpac`)
- the friend's report/PDF, or old report/PDF/presentation copies that are not part of the source-code repository

The supplied `.gitignore` excludes the common local/build/secret files, but always review the changes list before pushing. If an SMTP key was included in any shared backup, rotate it before continued use.

## Give your teacher the link

Open the repository page on GitHub and copy its browser URL, for example `https://github.com/YOUR-USERNAME/ASP-PARKMATE`. Replace `YOUR-USERNAME` with your actual username. Do not use this example URL as if it were real.

## Later updates

Edit the local repository files, review changes in GitHub Desktop, enter a meaningful Summary, click **Commit to main**, then click **Push origin**. The latest commit will appear on GitHub.
