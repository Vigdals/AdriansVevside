# Security

This document describes the current security design and operational practices for the **AdriansVevside** application.

The goal is:
- No secrets in source control.
- Clear separation between local development and production.
- Predictable and auditable access to data and services.

---

## 1. Architecture overview

- ASP.NET Core 8 MVC application.
- Hosted on **Azure App Service**.
- Data stored in **Azure SQL Database** (`Adrians_db`) in production.
- Local development uses **SQL Server LocalDB**.
- Outbound HTTP to external APIs (e.g. football-data.org) via typed `HttpClient`.

---

## 2. Environments

### Development

- Runs locally on the developer machine.
- Uses `ConnectionStrings:DefaultConnection` from `appsettings.json`:
  - `Server=(localdb)\MSSQLLocalDB;Database=vigdalsVevside;...`
- No production data should be used in development.
- API keys and other secrets should be provided via:
  - `dotnet user-secrets` **or**
  - environment variables.

### Production

- Runs on Azure App Service.
- Uses **Azure Key Vault** for all sensitive configuration.

---

## 3. Secrets and configuration

### 3.1 Data flow

- `appsettings.json` contains:
  - a non-sensitive local dev connection string (`DefaultConnection`).
  - `KeyVault:Url` (not a secret).
- On startup, `Program.cs`:

  1. Reads `KeyVault:Url` from configuration.
  2. Adds Azure Key Vault as a configuration provider using `DefaultAzureCredential`.
  3. Selects connection string based on environment:

     ```csharp
     if (builder.Environment.IsDevelopment())
     {
         connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
     }
     else
     {
         connectionString = builder.Configuration["db-connection-adriansvevside"];
     }
     ```

- In production, the database connection string is stored as a **secret** in Key Vault with the name:

  - `db-connection-adriansvevside`

- The App Service uses **Managed Identity** (or a dedicated service principal) with the role:
  - `Key Vault Secrets User` (or equivalent) on the vault.

### 3.2 Guidelines

- Never commit secrets (passwords, tokens, connection strings) to Git.
- All long-lived secrets must be stored in **Azure Key Vault**.
- Short-lived local secrets for development should be stored using:
  - `dotnet user-secrets`, or
  - environment variables, never in `appsettings.*.json` that is checked in.

### 3.3 FootballData API key

- Configuration section `FootballData` is bound to `FootballDataOptions`.
- `FootballData:ApiUrl` is safe to keep in `appsettings.json`.
- The **API key** must be provided via one of these mechanisms:
  - As a secret in Key Vault (`FootballData-ApiKey`) and read into configuration.
  - As an environment variable: `FootballData__ApiKey`.
  - As a user-secret in development.

In code, only `options.ApiKey` from configuration should be used; the key must never be hard-coded.

---

## 4. Database security

### Production (Azure SQL – `Adrians_db`)

- Connection string is only stored in Key Vault.
- Access from the application should use:
  - Managed Identity if possible, otherwise a dedicated SQL user with minimum permissions.
- Firewall:
  - Allow traffic only from:
    - App Service (via service tag / VNet / private endpoint).
    - Controlled admin IPs for maintenance.
- Recommended:
  - Enable Transparent Data Encryption (TDE) on the database.
  - Enable auditing / threat detection in Azure SQL.

### Development

- LocalDB is for local use only and does not contain production data.
- Backups and dumps from production must be anonymized before use in dev/test.

---

## 5. Authentication and authorization

- Uses ASP.NET Core Identity for local user accounts.
- `RequireConfirmedAccount` is currently `false`:
  - This is acceptable for a personal site, but consider enabling confirmation if you expose registration publicly.
- Roles are enabled; use roles for any administrative functions.
- All admin-only endpoints must be protected with `[Authorize(Roles = "...")]`.

---

## 6. Logging and monitoring

- Uses ASP.NET Core logging (configured via `Logging` section).
- Logs must not contain:
  - passwords
  - tokens
  - full connection strings
  - other sensitive personal data
- For production:
  - Use Application Insights or another log sink with restricted access.
  - Ensure retention policies are configured.

---

## 7. Dependencies and supply chain

- All dependencies are managed via NuGet.
- Security practices:
  - Keep .NET SDK and NuGet packages up to date.
  - Use Dependabot / GitHub security alerts for this repository.
  - Avoid unmaintained or low-trust packages.
  - No direct use of `npm` or frontend build tooling in this project for now; if introduced, the same rules apply.

---

## 8. Deployment and operations

- Deployment target: Azure App Service (Windows).
- Build configuration: `Release`, target framework `net8.0`.
- App Service configuration should **not** contain any connection strings; they are replaced by Key Vault.
- Required App Settings in production:
  - `ASPNETCORE_ENVIRONMENT = Production`
  - `KeyVault__Url = https://kv-vigdalsvault.vault.azure.net/`
- Before each deployment:
  - Verify that migrations have been applied to the Azure SQL database (via `dotnet ef database update` or CI pipeline).
  - Verify that App Service managed identity still has access to Key Vault.

---

## 9. Hardening checklist

- [ ] No secrets in Git (search for `Password=`, `User ID=`, tokens, etc.).
- [ ] All long-lived secrets stored in Key Vault.
- [ ] Managed Identity enabled on App Service and granted Key Vault access.
- [ ] Azure SQL firewall restricted to App Service + admin IPs.
- [ ] Identity admin endpoints protected with authorization.
- [ ] Logging does not include sensitive data.
- [ ] Dependencies scanned regularly (Dependabot / GitHub Advanced Security).
