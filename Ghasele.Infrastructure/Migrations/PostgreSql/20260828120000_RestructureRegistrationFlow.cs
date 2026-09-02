using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RestructureRegistrationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The phone-first flow creates a PendingRegistration with only a phone number and an
            // OTP; the name and password arrive only after the code is confirmed, so both columns
            // become nullable and a verified flag is added.
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "PendingRegistrations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "PendingRegistrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtpVerified",
                table: "PendingRegistrations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop rows that would violate the restored NOT NULL constraints (signups still at the
            // phone/OTP stage).
            migrationBuilder.Sql(
                "DELETE FROM \"PendingRegistrations\" WHERE \"PasswordHash\" IS NULL OR \"FullName\" IS NULL;");

            migrationBuilder.DropColumn(
                name: "IsOtpVerified",
                table: "PendingRegistrations");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "PendingRegistrations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "PendingRegistrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
