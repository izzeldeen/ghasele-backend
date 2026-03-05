# Multi-Database Provider Setup - Complete Guide

## 🎯 Overview

This .NET backend project supports **both SQL Server and PostgreSQL** as database providers. You can switch between them by simply changing a single line in the configuration file - no code changes required!

## 📦 Installed Packages

The following NuGet packages have been added to support both providers:

- **Npgsql.EntityFrameworkCore.PostgreSQL** (v9.0.2) - PostgreSQL provider for EF Core
- **Microsoft.EntityFrameworkCore.SqlServer** - SQL Server provider for EF Core

## ⚙️ Configuration

### appsettings.Development.json

```json
{
  "DatabaseSettings": {
    "Provider": "SqlServer",  // Change to "PostgreSql" to use PostgreSQL
    "SqlServerConnection": "Data Source=THINKPADE16512U;Initial Catalog=Ghasele;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;",
    "PostgreSqlConnection": "Host=localhost;Port=5432;Database=Ghasele;Username=postgres;Password=1234"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyForDevelopmentOnly12345!",
    "Issuer": "GhaseleAPI",
    "Audience": "GhaseleClient",
    "ExpiryMinutes": "60"
  }
}
```

### Switching Database Providers

Simply change the `Provider` value:
- For SQL Server: `"Provider": "SqlServer"`
- For PostgreSQL: `"Provider": "PostgreSql"` (or `"Postgres"`)

## 🚀 Quick Start

### Option 1: Using SQL Server (Default)

1. Ensure SQL Server is installed and running
2. Configuration is already set for SQL Server
3. Run the application:
   ```bash
   dotnet run --project Ghasele.API
   ```

### Option 2: Using PostgreSQL

1. **Install PostgreSQL** (if not already installed)
   - Download from: https://www.postgresql.org/download/
   - Default port: 5432
   - Remember your postgres user password

2. **Create the database:**
   ```sql
   -- Connect to PostgreSQL using pgAdmin or psql
   CREATE DATABASE Ghasele;
   ```

3. **Update connection string in appsettings.Development.json:**
   ```json
   "DatabaseSettings": {
     "Provider": "PostgreSql",
     "PostgreSqlConnection": "Host=localhost;Port=5432;Database=Ghasele;Username=postgres;Password=YOUR_PASSWORD"
   }
   ```

4. **Create PostgreSQL migrations:**
   ```powershell
   # Using the helper script
   .\create-postgres-migration.ps1
   
   # Or manually
   dotnet ef migrations add InitialPostgreSql -p Ghasele.Infrastructure -s Ghasele.API -o Migrations/PostgreSql
   ```

5. **Apply migrations:**
   ```powershell
   # Using the helper script
   .\apply-postgres-migration.ps1
   
   # Or manually
   dotnet ef database update -p Ghasele.Infrastructure -s Ghasele.API
   ```

6. **Run the application:**
   ```bash
   dotnet run --project Ghasele.API
   ```

## 📁 Migration Organization

Migrations are organized in separate folders:

```
Ghasele.Infrastructure/
├── Migrations/               # SQL Server migrations
│   ├── 20260206074720_InitialCreate.cs
│   ├── 20260206222911_AddOrdersTable.cs
│   └── ...
└── Migrations/PostgreSql/    # PostgreSQL migrations
    ├── InitialPostgreSql.cs
    └── ...
```

## 🔧 Manual Migration Commands

### For SQL Server:

```bash
# Create migration
dotnet ef migrations add MigrationName -p Ghasele.Infrastructure -s Ghasele.API

# Apply migration
dotnet ef database update -p Ghasele.Infrastructure -s Ghasele.API

# Revert migration
dotnet ef database update PreviousMigrationName -p Ghasele.Infrastructure -s Ghasele.API

# Remove last migration
dotnet ef migrations remove -p Ghasele.Infrastructure -s Ghasele.API
```

### For PostgreSQL:

```bash
# IMPORTANT: Set Provider to "PostgreSql" in appsettings.Development.json first!

# Create migration
dotnet ef migrations add MigrationName -p Ghasele.Infrastructure -s Ghasele.API -o Migrations/PostgreSql

# Apply migration
dotnet ef database update -p Ghasele.Infrastructure -s Ghasele.API

# Revert migration
dotnet ef database update PreviousMigrationName -p Ghasele.Infrastructure -s Ghasele.API

# Remove last migration
dotnet ef migrations remove -p Ghasele.Infrastructure -s Ghasele.API
```

## 💡 Helper Scripts

Two PowerShell scripts are provided for convenience:

### create-postgres-migration.ps1
- Automatically switches to PostgreSQL provider
- Creates migration in PostgreSQL folder
- Restores original provider setting

Usage:
```powershell
.\create-postgres-migration.ps1
```

### apply-postgres-migration.ps1
- Applies pending migrations to PostgreSQL
- Shows connection string before applying
- Asks for confirmation

Usage:
```powershell
.\apply-postgres-migration.ps1
```

## 🔍 How It Works

### Program.cs

The dynamic provider selection is implemented in `Program.cs`:

```csharp
// Database Configuration with dynamic provider selection
var databaseProvider = builder.Configuration["DatabaseSettings:Provider"] ?? "SqlServer";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (databaseProvider.ToLower())
    {
        case "postgresql":
        case "postgres":
            var postgresConnection = builder.Configuration["DatabaseSettings:PostgreSqlConnection"];
            options.UseNpgsql(postgresConnection, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
            });
            break;

        case "sqlserver":
        default:
            var sqlServerConnection = builder.Configuration["DatabaseSettings:SqlServerConnection"];
            options.UseSqlServer(sqlServerConnection);
            break;
    }
});
```

### ApplicationDbContext

The DbContext has been updated to use provider-agnostic configurations:

- Changed from `HasPrecision(18, 2)` to `HasColumnType("decimal(18,2)")`
- Both SQL Server and PostgreSQL support this decimal type
- No SQL Server-specific features used

## 🎯 Data Type Compatibility

| .NET Type | SQL Server | PostgreSQL |
|-----------|------------|------------|
| `decimal(18,2)` | `decimal(18,2)` | `numeric(18,2)` |
| `string` | `nvarchar(max)` | `text` |
| `DateTime` | `datetime2` | `timestamp` |
| `Guid` | `uniqueidentifier` | `uuid` |
| `int` | `int` | `integer` |
| `bool` | `bit` | `boolean` |

## 🐛 Troubleshooting

### PostgreSQL Connection Issues

1. **"Connection refused"**
   - Verify PostgreSQL is running: `pg_isready`
   - Check port 5432 is accessible

2. **"Authentication failed"**
   - Verify username and password in connection string
   - Check `pg_hba.conf` for authentication methods

3. **"Database does not exist"**
   - Create database manually first:
     ```sql
     CREATE DATABASE Ghasele;
     ```

### Migration Issues

1. **"No migrations found"**
   - Ensure you're using the correct migrations folder
   - For PostgreSQL, migrations should be in `Migrations/PostgreSql/`

2. **"Migration already applied"**
   - Check `__EFMigrationsHistory` table in your database
   - You may need to remove the entry manually or rollback

3. **"Provider mismatch"**
   - Ensure `Provider` in appsettings matches your target database
   - Rebuild the project after changing provider

## 📊 Testing Both Providers

To ensure your application works with both providers:

1. **Test with SQL Server:**
   ```json
   "Provider": "SqlServer"
   ```
   Run migrations and test the API

2. **Test with PostgreSQL:**
   ```json
   "Provider": "PostgreSql"
   ```
   Run migrations and test the API

3. **Compare Results:**
   - Both databases should have identical schemas
   - API should behave identically
   - Check data types match expected formats

## 🔐 Production Deployment

For production:

1. Update `appsettings.json` (not Development):
   ```json
   "DatabaseSettings": {
     "Provider": "PostgreSql",
     "SqlServerConnection": "Your-Production-SQL-Connection",
     "PostgreSqlConnection": "Your-Production-PostgreSQL-Connection"
   }
   ```

2. Use environment variables for sensitive data:
   ```bash
   export DatabaseSettings__Provider="PostgreSql"
   export DatabaseSettings__PostgreSqlConnection="Host=prod-db;..."
   ```

3. Never commit passwords or production connection strings to source control!

## 📚 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/efcore/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/)

## ✅ Checklist

Before switching providers:

- [ ] Database server is installed and running
- [ ] Database is created
- [ ] Connection string is correct in appsettings
- [ ] Provider setting matches target database
- [ ] Migrations have been created
- [ ] Migrations have been applied
- [ ] Application builds successfully
- [ ] API endpoints tested and working

---

**Need help?** Check `POSTGRESQL_MIGRATION_GUIDE.md` for detailed migration instructions.
