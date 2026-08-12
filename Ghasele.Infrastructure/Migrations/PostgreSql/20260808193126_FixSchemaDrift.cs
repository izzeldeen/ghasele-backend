using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <summary>
    /// Repairs a database whose schema drifted away from the migration history.
    ///
    /// "__EFMigrationsHistory" reported every migration as applied, but the
    /// Drivers and MarketingCodes tables were absent, along with several
    /// columns on Cleaners and Orders and the constraints/indexes that depend
    /// on them. Because the model and the snapshot still agree, `migrations
    /// add` produced an empty diff - EF had no way to see the drift. Every
    /// statement below is therefore written to be idempotent so this migration
    /// is a no-op against a database that was never damaged.
    /// </summary>
    public partial class FixSchemaDrift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Drivers"" (
                    ""Id"" uuid NOT NULL,
                    ""Name"" character varying(100) NOT NULL,
                    ""PhoneNumber"" character varying(20) NOT NULL,
                    ""Note"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_Drivers"" PRIMARY KEY (""Id"")
                );");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""MarketingCodes"" (
                    ""Id"" uuid NOT NULL,
                    ""Code"" character varying(50) NOT NULL,
                    ""DiscountPercentage"" numeric(5,2) NOT NULL,
                    ""SharePercentage"" numeric(5,2) NOT NULL,
                    ""MarketerName"" character varying(100) NOT NULL,
                    ""IsActive"" boolean NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_MarketingCodes"" PRIMARY KEY (""Id"")
                );");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_MarketingCodes_Code""
                    ON ""MarketingCodes"" (""Code"");");

            // Existing rows force a default on the non-nullable additions; the
            // model declares none, so the defaults are dropped again once the
            // backfill is done.
            migrationBuilder.Sql(@"
                ALTER TABLE ""Cleaners""
                    ADD COLUMN IF NOT EXISTS ""CleaningLocation"" text NULL,
                    ADD COLUMN IF NOT EXISTS ""Latitude"" double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS ""Longitude"" double precision NOT NULL DEFAULT 0;
                ALTER TABLE ""Cleaners""
                    ALTER COLUMN ""Latitude"" DROP DEFAULT,
                    ALTER COLUMN ""Longitude"" DROP DEFAULT;");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders""
                    ADD COLUMN IF NOT EXISTS ""MarketingCodeId"" uuid NULL,
                    ADD COLUMN IF NOT EXISTS ""MarketingDiscount"" numeric(18,2) NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS ""MarketerShare"" numeric(18,2) NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS ""MarketingDiscountPercentage"" numeric NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS ""MarketerSharePercentage"" numeric NOT NULL DEFAULT 0;
                ALTER TABLE ""Orders""
                    ALTER COLUMN ""MarketingDiscount"" DROP DEFAULT,
                    ALTER COLUMN ""MarketerShare"" DROP DEFAULT,
                    ALTER COLUMN ""MarketingDiscountPercentage"" DROP DEFAULT,
                    ALTER COLUMN ""MarketerSharePercentage"" DROP DEFAULT;");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_Trips_AssignedDriverId""
                    ON ""Trips"" (""AssignedDriverId"");
                CREATE INDEX IF NOT EXISTS ""IX_Orders_MarketingCodeId""
                    ON ""Orders"" (""MarketingCodeId"");");

            // ADD CONSTRAINT has no IF NOT EXISTS, so guard on pg_constraint.
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint
                        WHERE conname = 'FK_Trips_Drivers_AssignedDriverId'
                    ) THEN
                        ALTER TABLE ""Trips""
                            ADD CONSTRAINT ""FK_Trips_Drivers_AssignedDriverId""
                            FOREIGN KEY (""AssignedDriverId"") REFERENCES ""Drivers"" (""Id"")
                            ON DELETE SET NULL;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint
                        WHERE conname = 'FK_Orders_MarketingCodes_MarketingCodeId'
                    ) THEN
                        ALTER TABLE ""Orders""
                            ADD CONSTRAINT ""FK_Orders_MarketingCodes_MarketingCodeId""
                            FOREIGN KEY (""MarketingCodeId"") REFERENCES ""MarketingCodes"" (""Id"")
                            ON DELETE SET NULL;
                    END IF;
                END $$;");

            // RemoveTripType dropped Trips.Type from the model but the column
            // survived in the database as NOT NULL with no default, so every
            // insert of a Trip would fail on a column EF no longer knows about.
            migrationBuilder.Sql(@"ALTER TABLE ""Trips"" DROP COLUMN IF EXISTS ""Type"";");

            // UpdateStatuses' legacy conversions never reached these rows.
            // 'Completed' is not a member of TripStatus, so reading a Trip threw
            // before this ran. Re-applying is safe: the WHERE clauses no-op once
            // the values are already current.
            migrationBuilder.Sql(@"
                UPDATE ""Trips""  SET ""Status"" = 'Assigned'          WHERE ""Status"" = 'Created';
                UPDATE ""Trips""  SET ""Status"" = 'Delivered'         WHERE ""Status"" = 'Completed';
                UPDATE ""Orders"" SET ""Status"" = 'PendingCollection' WHERE ""Status"" = 'Pending';
                UPDATE ""Orders"" SET ""Status"" = 'Cleaning'          WHERE ""Status"" = 'InProgress';
                UPDATE ""Orders"" SET ""Status"" = 'Ready'             WHERE ""Status"" = 'Completed';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty. Everything this migration creates belongs to
            // PostgresInitial; dropping it on revert would re-introduce the very
            // drift being repaired.
        }
    }
}
