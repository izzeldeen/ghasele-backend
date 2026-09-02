using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddDeliveryWindows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryWindows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 20),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryWindows", x => x.Id);
                });

            // The three windows the operator asked for out of the box; fully editable from the
            // dashboard afterwards (13:00-14:00, 15:00-16:00, 19:00-20:00).
            migrationBuilder.InsertData(
                table: "DeliveryWindows",
                columns: new[] { "Id", "Capacity", "CreatedAt", "EndTime", "IsActive", "StartTime" },
                values: new object[,]
                {
                    { new Guid("d0000001-0000-4000-8000-000000000001"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new TimeOnly(14, 0, 0), true, new TimeOnly(13, 0, 0) },
                    { new Guid("d0000001-0000-4000-8000-000000000002"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new TimeOnly(16, 0, 0), true, new TimeOnly(15, 0, 0) },
                    { new Guid("d0000001-0000-4000-8000-000000000003"), 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new TimeOnly(20, 0, 0), true, new TimeOnly(19, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryWindows_StartTime",
                table: "DeliveryWindows",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DeliveryWindows");
        }
    }
}
