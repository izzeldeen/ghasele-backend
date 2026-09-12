using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddOrderTripSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryWindowId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ScheduledDate",
                table: "Orders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ScheduledEndTime",
                table: "Orders",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ScheduledStartTime",
                table: "Orders",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryWindowId_ScheduledDate",
                table: "Orders",
                columns: new[] { "DeliveryWindowId", "ScheduledDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryWindows_DeliveryWindowId",
                table: "Orders",
                column: "DeliveryWindowId",
                principalTable: "DeliveryWindows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryWindows_DeliveryWindowId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveryWindowId_ScheduledDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryWindowId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ScheduledEndTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ScheduledStartTime",
                table: "Orders");
        }
    }
}
