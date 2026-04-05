using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Orders Legacy
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"Status\" = 'PendingCollection' WHERE \"Status\" = 'Pending';");
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"Status\" = 'Cleaning' WHERE \"Status\" = 'InProgress';");
            migrationBuilder.Sql("UPDATE \"Orders\" SET \"Status\" = 'Ready' WHERE \"Status\" = 'Completed';");
            
            // Trips Legacy
            migrationBuilder.Sql("UPDATE \"Trips\" SET \"Status\" = 'Assigned' WHERE \"Status\" = 'Created';");
            migrationBuilder.Sql("UPDATE \"Trips\" SET \"Status\" = 'Delivered' WHERE \"Status\" = 'Completed';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
