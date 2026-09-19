using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddUserIsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Without this every existing account lands on false, and the dashboard refuses to
            // sign anyone in: its login checks AuthResponse.IsAdmin, which is this column. The
            // accounts that could reach the dashboard before this column existed are exactly the
            // ones whose role is Admin, so they are the ones carried over.
            //
            // Role is stored as text (HasConversion<string>), not as the enum's ordinal.
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""IsAdmin"" = TRUE WHERE ""Role"" = 'Admin';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");
        }
    }
}
