-- ============================================================================
--  Purge EVERYTHING - full database reset back to an empty application
-- ============================================================================
--  Target : PostgreSQL (Npgsql)
--  Usage  : psql "$CONNECTION_STRING" -f scripts/purge-all.sql
--
--  SAFETY: this script ends in ROLLBACK. Nothing is deleted until you change
--          the last line to COMMIT. Run it as-is first and read the two count
--          reports - the "after" report shows exactly what the COMMIT would do.
--
--  *** THIS DELETES EVERY CUSTOMER, ORDER, DRIVER AND TICKET. ***
--  *** There is no undo. Take a backup before the COMMIT run:              ***
--  ***   pg_dump "$CONNECTION_STRING" > backup-$(date +%F-%H%M).sql        ***
--
--  Scope
--  -----
--  Section 1 removes all operational and customer data.
--  Section 2 removes the reference data the app needs to function at all
--  (service catalogue, delivery windows, settings). It is included because
--  this script is "delete everything", but see the warning above it - after
--  running it the app has no item types to order and no settings to read,
--  so it must be reseeded before the app is usable. To keep that data,
--  delete or comment out Section 2 and its lines in the count reports.
--
--  NEVER delete "__EFMigrationsHistory". It is not touched here and must not
--  be: EF Core would treat the database as brand new and try to re-run every
--  migration against tables that already exist.
--
--  Schema notes (verified against ApplicationDbContext)
--  ----------------------------------------------------
--    * OrderItems.OrderId      -> Orders.Id          ON DELETE CASCADE
--    * Orders.UserId           -> Users.Id           ON DELETE CASCADE
--    * Orders.TripId           -> Trips.Id           ON DELETE SET NULL
--    * Orders.MarketingCodeId  -> MarketingCodes.Id  ON DELETE SET NULL
--    * Trips.CleanerId         -> Cleaners.Id        ON DELETE SET NULL
--    * Trips.AssignedDriverId  -> Drivers.Id         ON DELETE SET NULL
--    * UserLocations.UserId    -> Users.Id           ON DELETE CASCADE
--    * Notifications.UserId    -> Users.Id           ON DELETE CASCADE
--    * Drivers.UserId          -> Users.Id           ON DELETE SET NULL
--    * SupportTickets.UserId is a plain string column with NO foreign key.
--    * AuditLogs has no foreign key either - it is request logging.
--
--  Deletes run children-first anyway rather than relying on CASCADE, so the
--  row counts in the report are the real per-table numbers.
--
--  Keys are uuid everywhere except SupportTickets.Id, which is an identity
--  column and is the only sequence that needs resetting.
-- ============================================================================

BEGIN;

-- --- Before -----------------------------------------------------------------
SELECT 'BEFORE' AS phase, 'OrderItems' AS tbl, count(*) AS rows FROM "OrderItems"
UNION ALL SELECT 'BEFORE', 'Orders',               count(*) FROM "Orders"
UNION ALL SELECT 'BEFORE', 'Trips',                count(*) FROM "Trips"
UNION ALL SELECT 'BEFORE', 'Drivers',              count(*) FROM "Drivers"
UNION ALL SELECT 'BEFORE', 'Cleaners',             count(*) FROM "Cleaners"
UNION ALL SELECT 'BEFORE', 'UserLocations',        count(*) FROM "UserLocations"
UNION ALL SELECT 'BEFORE', 'Notifications',        count(*) FROM "Notifications"
UNION ALL SELECT 'BEFORE', 'SupportTickets',       count(*) FROM "SupportTickets"
UNION ALL SELECT 'BEFORE', 'AuditLogs',            count(*) FROM "AuditLogs"
UNION ALL SELECT 'BEFORE', 'PendingRegistrations', count(*) FROM "PendingRegistrations"
UNION ALL SELECT 'BEFORE', 'Users',                count(*) FROM "Users"
UNION ALL SELECT 'BEFORE', 'MarketingCodes',       count(*) FROM "MarketingCodes"
UNION ALL SELECT 'BEFORE', 'ItemTypes',            count(*) FROM "ItemTypes"
UNION ALL SELECT 'BEFORE', 'DeliveryWindows',      count(*) FROM "DeliveryWindows"
UNION ALL SELECT 'BEFORE', 'AppSettings',          count(*) FROM "AppSettings"
ORDER BY tbl;

-- ============================================================================
--  Section 1 - operational and customer data
-- ============================================================================
-- Children first. Orders must go before Trips because Orders.TripId is
-- SET NULL rather than cascading, and Drivers/Cleaners must go after Trips
-- for the same reason.
DELETE FROM "OrderItems";
DELETE FROM "Orders";
DELETE FROM "Trips";
DELETE FROM "Drivers";
DELETE FROM "Cleaners";

-- Everything below hangs off Users, so Users goes last.
DELETE FROM "UserLocations";
DELETE FROM "Notifications";
DELETE FROM "SupportTickets";
DELETE FROM "AuditLogs";
DELETE FROM "PendingRegistrations";
DELETE FROM "Users";

-- SupportTickets.Id is the only identity column in the schema; without this
-- the next ticket carries on from the old numbering instead of starting at 1.
ALTER SEQUENCE IF EXISTS "SupportTickets_Id_seq" RESTART WITH 1;

-- ============================================================================
--  Section 2 - reference data
-- ============================================================================
--  *** WARNING: this is the data the app needs to run. ***
--  With ItemTypes empty the customer app has no services to order; with
--  AppSettings empty the pricing and delivery fee lookups have nothing to
--  read. Delete this section if you only want a customer/order reset.
--
--  Attachment files under wwwroot/uploads/support/ are NOT removed by this
--  script - SQL cannot touch the filesystem. After a COMMIT those files are
--  orphaned and can be deleted separately.
-- ============================================================================
DELETE FROM "MarketingCodes";
DELETE FROM "ItemTypes";
DELETE FROM "DeliveryWindows";
DELETE FROM "AppSettings";

-- --- After ------------------------------------------------------------------
SELECT 'AFTER' AS phase, 'OrderItems' AS tbl, count(*) AS rows FROM "OrderItems"
UNION ALL SELECT 'AFTER', 'Orders',               count(*) FROM "Orders"
UNION ALL SELECT 'AFTER', 'Trips',                count(*) FROM "Trips"
UNION ALL SELECT 'AFTER', 'Drivers',              count(*) FROM "Drivers"
UNION ALL SELECT 'AFTER', 'Cleaners',             count(*) FROM "Cleaners"
UNION ALL SELECT 'AFTER', 'UserLocations',        count(*) FROM "UserLocations"
UNION ALL SELECT 'AFTER', 'Notifications',        count(*) FROM "Notifications"
UNION ALL SELECT 'AFTER', 'SupportTickets',       count(*) FROM "SupportTickets"
UNION ALL SELECT 'AFTER', 'AuditLogs',            count(*) FROM "AuditLogs"
UNION ALL SELECT 'AFTER', 'PendingRegistrations', count(*) FROM "PendingRegistrations"
UNION ALL SELECT 'AFTER', 'Users',                count(*) FROM "Users"
UNION ALL SELECT 'AFTER', 'MarketingCodes',       count(*) FROM "MarketingCodes"
UNION ALL SELECT 'AFTER', 'ItemTypes',            count(*) FROM "ItemTypes"
UNION ALL SELECT 'AFTER', 'DeliveryWindows',      count(*) FROM "DeliveryWindows"
UNION ALL SELECT 'AFTER', 'AppSettings',          count(*) FROM "AppSettings"
ORDER BY tbl;

-- ============================================================================
--  ARMED. This COMMIT is permanent - there is no undo.
--
--  Change back to ROLLBACK to return the script to a safe dry run.
--
--  Note: deleting this line rather than replacing it does NOT commit. The
--  BEGIN above stays open, psql disconnects at end of file, and PostgreSQL
--  rolls the whole transaction back - which looks exactly like "nothing
--  happened". It has to say COMMIT.
-- ============================================================================
COMMIT;
