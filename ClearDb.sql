-- ============================================================================
-- ClearDb.sql
-- Deletes all Orders, OrderItems and Trips from the Ghasele database.
--
-- Provider : PostgreSQL (DatabaseSettings:Provider = "postgresql")
-- Usage    : psql -h <host> -U <user> -d <database> -f ClearDb.sql
--
-- WARNING  : This permanently removes operational data. Take a backup first:
--            pg_dump -h <host> -U <user> -d <database> -F c -f backup.dump
--
-- Delete order matters (child -> parent):
--   1. "OrderItems"  (FK OrderId -> Orders, ON DELETE CASCADE)
--   2. "Orders"      (FK TripId  -> Trips,  ON DELETE SET NULL)
--   3. "Trips"
-- Nothing else in the schema references these three tables, so no other
-- table needs to be touched. Users, Cleaners, Drivers, ItemTypes,
-- DeliveryWindows, MarketingCodes and settings are all left intact.
-- ============================================================================

BEGIN;

-- 1. Order items (children of Orders)
DELETE FROM "OrderItems";

-- 2. Orders (children of Trips)
DELETE FROM "Orders";

-- 3. Trips
DELETE FROM "Trips";

-- Sanity check: all three must report 0.
SELECT 'OrderItems' AS table_name, COUNT(*) AS remaining_rows FROM "OrderItems"
UNION ALL
SELECT 'Orders',     COUNT(*) FROM "Orders"
UNION ALL
SELECT 'Trips',      COUNT(*) FROM "Trips";

COMMIT;
-- ROLLBACK;  -- use instead of COMMIT to dry-run the script

-- ============================================================================
-- Faster alternative (whole-table wipe, resets nothing else)
-- TRUNCATE ignores row-level FK cascades, so all three tables must be listed
-- together in a single statement.
-- ============================================================================
-- BEGIN;
-- TRUNCATE TABLE "OrderItems", "Orders", "Trips";
-- COMMIT;

-- ============================================================================
-- Targeted deletes - uncomment the block you need instead of the wipe above.
-- ============================================================================

-- --- Delete a single order (and its items) by Id -----------------------------
-- BEGIN;
-- DELETE FROM "OrderItems" WHERE "OrderId" = '00000000-0000-0000-0000-000000000000';
-- DELETE FROM "Orders"     WHERE "Id"      = '00000000-0000-0000-0000-000000000000';
-- COMMIT;

-- --- Delete a single trip, its orders and their items ------------------------
-- BEGIN;
-- DELETE FROM "OrderItems"
--  WHERE "OrderId" IN (SELECT "Id" FROM "Orders"
--                       WHERE "TripId" = '00000000-0000-0000-0000-000000000000');
-- DELETE FROM "Orders" WHERE "TripId" = '00000000-0000-0000-0000-000000000000';
-- DELETE FROM "Trips"  WHERE "Id"     = '00000000-0000-0000-0000-000000000000';
-- COMMIT;

-- --- Delete every order belonging to one user --------------------------------
-- BEGIN;
-- DELETE FROM "OrderItems"
--  WHERE "OrderId" IN (SELECT "Id" FROM "Orders"
--                       WHERE "UserId" = '00000000-0000-0000-0000-000000000000');
-- DELETE FROM "Orders" WHERE "UserId" = '00000000-0000-0000-0000-000000000000';
-- COMMIT;

-- --- Delete test/old data older than a cut-off date --------------------------
-- BEGIN;
-- DELETE FROM "OrderItems"
--  WHERE "OrderId" IN (SELECT "Id" FROM "Orders" WHERE "CreatedAt" < '2026-01-01');
-- DELETE FROM "Orders" WHERE "CreatedAt" < '2026-01-01';
-- DELETE FROM "Trips"  WHERE "CreatedAt" < '2026-01-01'
--    AND "Id" NOT IN (SELECT "TripId" FROM "Orders" WHERE "TripId" IS NOT NULL);
-- COMMIT;

-- --- Orphan trips only (trips that no longer have any orders) ----------------
-- BEGIN;
-- DELETE FROM "Trips" t
--  WHERE NOT EXISTS (SELECT 1 FROM "Orders" o WHERE o."TripId" = t."Id");
-- COMMIT;
