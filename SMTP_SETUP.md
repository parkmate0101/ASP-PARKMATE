# Local Brevo SMTP setup

The email service reads the following settings from configuration section `EmailSettings`:

- `SmtpHost`
- `SmtpPort`
- `SmtpUsername`
- `SmtpPassword`
- `FromEmail`
- `FromName`

For local development:

1. Copy `appsettings.Development.example.json` to `appsettings.Development.json`.
2. Replace `YOUR_BREVO_SMTP_LOGIN`, `YOUR_BREVO_SMTP_KEY`, and `YOUR_VERIFIED_SENDER_EMAIL` with your own account values.
3. Keep `appsettings.Development.json` local. It is excluded by `.gitignore`.
4. Restart the application and test registration OTP, OTP login, and password-reset OTP.

Do not paste SMTP credentials into chat, reports, screenshots, source control, or a public repository. If a real SMTP key was shared in a project archive, rotate it before continued use.
