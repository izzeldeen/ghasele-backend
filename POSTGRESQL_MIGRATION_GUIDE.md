# PostgreSQL Migration Guide

## Overview
This guide explains how to work with PostgreSQL migrations in this project, which supports both SQL Server and PostgreSQL databases dynamically.

## Configuration

The database provider is selected in `appsettings.Development.json` or `appsettings.json`:

```json
"DatabaseSettings": {
  "Provider": "SqlServer",  // or "PostgreSql"
  "SqlServerConnection": "Data Source=...;Initial Catalog=Ghasele;...",
  "PostgreSqlConnection": "Host=localhost;Port=5432;Database=Ghasele;Username=postgres;Password=1234"
}
```

## PostgreSQL Database Setup (Auto-Initialization)

Currently, due to tooling compatibility issues, manual migrations are **disabled** for PostgreSQL. instead, we use **Auto-Initialization**.

### How it works:
1. Set `"Provider": "PostgreSql"` in `appsettings.Development.json`.
2. Run the application: `dotnet run --project Ghasele.API`.
3. The application will automatically detect PostgreSQL and create the database schema using `EnsureCreated()` at startup.

**No manual `dotnet ef migrations add` commands are needed for PostgreSQL.**

### Switching back to SQL Server
SQL Server migrations continue to work normally using the standard commands.


## Important Notes

1. **Separate Migration Folders**: SQL Server migrations are in `Migrations/`, PostgreSQL migrations are in `Migrations/PostgreSql/`

2. **Provider-Agnostic Code**: The `ApplicationDbContext` has been updated to use `HasColumnType("decimal(18,2)")` instead of `HasPrecision()` for maximum compatibility.

3. **Switching Providers**: Simply change the `Provider` value in appsettings.json - no code changes needed!

4. **Connection Strings**: 
   - SQL Server: Uses Windows Authentication by default
   - PostgreSQL: Uses username/password authentication

5. **Data Type Compatibility**:
   - `decimal(18,2)` works on both databases
   - `datetime2` (SQL Server) = `timestamp` (PostgreSQL)
   - `nvarchar` (SQL Server) = `varchar` (PostgreSQL)
   - GUID columns work on both

## Running the Application

### With SQL Server:
```json
"Provider": "SqlServer"
```

### With PostgreSQL:
```json
"Provider": "PostgreSql"
```

Then run:
```bash
dotnet run --project Ghasele.API
```

## Troubleshooting

1. **Migration conflict**: If you get conflicts, ensure you're using the correct `-o` (output) folder
2. **Connection refused**: Verify PostgreSQL is running: `pg_isready`
3. **Authentication failed**: Check your PostgreSQL username and password
4. **Database doesn't exist**: Create it manually first:
   ```sql
   CREATE DATABASE Ghasele;
   ```

## Database Commands

### PostgreSQL Database Setup (if needed)
```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE Ghasele;

# Grant permissions (if using different user)
GRANT ALL PRIVILEGES ON DATABASE Ghasele TO your_username;
```

### Check Current Provider
The application logs will show which provider is being used at startup.
