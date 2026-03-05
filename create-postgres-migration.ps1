# PostgreSQL Migration Script
# This script helps create migrations for PostgreSQL

Write-Host "PostgreSQL Migration Helper" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green
Write-Host ""

# Check if EF Core tools are installed
$efToolsInstalled = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efToolsInstalled) {
    Write-Host "Entity Framework Core tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "EF Core tools installed successfully!" -ForegroundColor Green
    Write-Host ""
}

# Get migration name from user
$migrationName = Read-Host "Enter migration name (e.g., InitialPostgreSql)"

if ([string]::IsNullOrWhiteSpace($migrationName)) {
    Write-Host "Error: Migration name cannot be empty!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Creating PostgreSQL migration: $migrationName" -ForegroundColor Cyan
Write-Host ""

# Update appsettings to use PostgreSQL
Write-Host "Step 1: Updating appsettings.Development.json to use PostgreSQL..." -ForegroundColor Yellow
$appsettingsPath = ".\Ghasele.API\appsettings.Development.json"
$appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
$originalProvider = $appsettings.DatabaseSettings.Provider
$appsettings.DatabaseSettings.Provider = "PostgreSql"
$appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
Write-Host "  ✓ Provider changed to PostgreSQL" -ForegroundColor Green

# Create PostgreSQL migrations folder if it doesn't exist
$postgresMigrationsFolder = ".\Ghasele.Infrastructure\Migrations\PostgreSql"
if (-not (Test-Path $postgresMigrationsFolder)) {
    Write-Host "Step 2: Creating PostgreSQL migrations folder..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $postgresMigrationsFolder -Force | Out-Null
    Write-Host "  ✓ Folder created" -ForegroundColor Green
} else {
    Write-Host "Step 2: PostgreSQL migrations folder already exists" -ForegroundColor Green
}

# Generate migration
Write-Host "Step 3: Generating migration..." -ForegroundColor Yellow
dotnet ef migrations add $migrationName `
    -p Ghasele.Infrastructure `
    -s Ghasele.API `
    -o Migrations/PostgreSql

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Migration created successfully!" -ForegroundColor Green
} else {
    Write-Host "  ✗ Migration failed!" -ForegroundColor Red
    # Restore original provider
    $appsettings.DatabaseSettings.Provider = $originalProvider
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    exit 1
}

# Restore original provider setting
Write-Host "Step 4: Restoring original provider setting..." -ForegroundColor Yellow
$appsettings.DatabaseSettings.Provider = $originalProvider
$appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
Write-Host "  ✓ Provider restored to: $originalProvider" -ForegroundColor Green

Write-Host ""
Write-Host "Migration created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the migration files in: Ghasele.Infrastructure\Migrations\PostgreSql\" -ForegroundColor White
Write-Host "2. To apply the migration, run: .\apply-postgres-migration.ps1" -ForegroundColor White
Write-Host ""
