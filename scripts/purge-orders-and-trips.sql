-- ============================================================================
--  Purge all order and trip data (OrderItems + Orders + Trips + order notifs)
-- ============================================================================
--  Target : PostgreSQL (Npgsql)
--  Usage  : psql "$CONNECTION_STRING" -f scripts/purge-orders-and-trips.sql
--
--  SAFETY: this script ends in ROLLBACK. Nothing is deleted until you change
--          the last line to COMMIT. Run it as-is first and read the two count
--          reports - the "after" report shows exactly what the COMMIT would do.
--
--  Difference from purge-orders.sql: that script clears orders only and leaves
--  trips behind as empty shells. This one clears trips and order notifications
--  as well.
--
--  Schema notes (verified against the entity model):
--    * OrderItems.OrderId     -> Orders.Id         ON DELETE CASCADE
--    * Orders.TripId          -> Trips.Id          ON DELETE SET NULL
--    * Orders.UserId          -> Users.Id          (Users are NOT touched)
--    * Orders.MarketingCodeId -> MarketingCodes.Id ON DELETE SET NULL
--    * Trips.AssignedDriverId -> Drivers.Id        (Drivers are NOT touched)
--    * Trips.CleanerId        -> Cleaners.Id       (Cleaners are NOT touched)
--    * AuditLogs has NO foreign key to Orders or Trips - it is request logging
--      and is left alone.
--    * SupportTickets have no foreign key to Orders either.
--    * All keys are uuid, so there are no identity sequences to reset.
--    * ReferenceNumber is generated in code ("GH-yyyyMMdd-XXXX"), so order and
--      trip numbering needs no reset.
--
--  NOT deleted by this script: Users, Drivers, Cleaners, MarketingCodes,
--  UserLocations, SupportTickets, ItemTypes, AuditLogs.
-- ============================================================================

BEGIN;

-- --- Before -----------------------------------------------------------------
SELECT 'BEFORE' AS phase, 'OrderItems' AS tbl, count(*) AS rows FROM "OrderItems"
UNION ALL SELECT 'BEFORE', 'Orders',            count(*) FROM "Orders"
UNION ALL SELECT 'BEFORE', 'Trips',             count(*) FROM "Trips"
UNION ALL SELECT 'BEFORE', 'Notifications',     count(*) FROM "Notifications"
UNION ALL SELECT 'BEFORE', 'Notifications(GH-)',
       count(*) FROM "Notifications" WHERE "Body" LIKE '%GH-%';

-- --- Delete -----------------------------------------------------------------
-- Order matters only for readability: OrderItems cascades from Orders, and
-- Orders.TripId is SET NULL rather than cascading, so Orders must go before
-- Trips to avoid leaving dangling references mid-transaction.
DELETE FROM "OrderItems";
DELETE FROM "Orders";
DELETE FROM "Trips";

-- --- Order notifications ------------------------------------------------------
-- Notifications have NO foreign key to Orders - only free text - so this is a
-- best-effort match on the generated "GH-yyyyMMdd-XXXX" reference prefix, not a
-- relational delete. Notifications that never quoted a reference number survive,
-- and the count report above shows exactly how many rows this removes.
DELETE FROM "Notifications" WHERE "Body" LIKE '%GH-%';

-- --- After ------------------------------------------------------------------
SELECT 'AFTER' AS phase, 'OrderItems' AS tbl, count(*) AS rows FROM "OrderItems"
UNION ALL SELECT 'AFTER', 'Orders',            count(*) FROM "Orders"
UNION ALL SELECT 'AFTER', 'Trips',             count(*) FROM "Trips"
UNION ALL SELECT 'AFTER', 'Notifications',     count(*) FROM "Notifications"
UNION ALL SELECT 'AFTER', 'Notifications(GH-)',
       count(*) FROM "Notifications" WHERE "Body" LIKE '%GH-%';

-- ============================================================================
--  Change to COMMIT to actually apply.
-- ============================================================================
ROLLBACK;
