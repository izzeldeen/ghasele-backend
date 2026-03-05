# Apply PostgreSQL Migration Script
# This script applies pending migrations to the PostgreSQL database

Write-Host "Apply PostgreSQL Migration" -ForegroundColor Green
Write-Host "==========================" -ForegroundColor Green
Write-Host ""

# Update appsettings to use PostgreSQL
Write-Host "Step 1: Configuring application to use PostgreSQL..." -ForegroundColor Yellow
$appsettingsPath = ".\Ghasele.API\appsettings.Development.json"
$appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
$originalProvider = $appsettings.DatabaseSettings.Provider
$appsettings.DatabaseSettings.Provider = "PostgreSql"
$appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
Write-Host "  ✓ Provider set to PostgreSQL" -ForegroundColor Green

# Display connection info
Write-Host ""
Write-Host "Connection String:" -ForegroundColor Cyan
Write-Host "  $($appsettings.DatabaseSettings.PostgreSqlConnection)" -ForegroundColor White
Write-Host ""

# Confirm with user
$confirmation = Read-Host "Do you want to apply migrations to this database? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    # Restore original provider
    $appsettings.DatabaseSettings.Provider = $originalProvider
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    exit 0
}

# Apply migrations
Write-Host ""
Write-Host "Step 2: Applying migrations to PostgreSQL database..." -ForegroundColor Yellow
dotnet ef database update -p Ghasele.Infrastructure -s Ghasele.API

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Migrations applied successfully!" -ForegroundColor Green
} else {
    Write-Host "  ✗ Migration failed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "  - PostgreSQL is not running" -ForegroundColor White
    Write-Host "  - Connection string is incorrect" -ForegroundColor White
    Write-Host "  - Database doesn't exist (create it manually first)" -ForegroundColor White
    Write-Host "  - Authentication failed (check username/password)" -ForegroundColor White
}

# Restore original provider setting
Write-Host ""
Write-Host "Step 3: Restoring original provider setting..." -ForegroundColor Yellow
$appsettings.DatabaseSettings.Provider = $originalProvider
$appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
Write-Host "  ✓ Provider restored to: $originalProvider" -ForegroundColor Green

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
Write-Host ""
