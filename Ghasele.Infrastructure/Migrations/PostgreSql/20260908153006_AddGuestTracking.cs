using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddGuestTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SupportTickets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhoneNumber",
                table: "SupportTickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "SupportTickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_DeviceToken",
                table: "SupportTickets",
                column: "DeviceToken");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeviceToken",
                table: "Orders",
                column: "DeviceToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_DeviceToken",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeviceToken",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ContactPhoneNumber",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SupportTickets",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
