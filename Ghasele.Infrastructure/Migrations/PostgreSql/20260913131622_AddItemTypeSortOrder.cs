using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddItemTypeSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ItemTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-111111111111"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                column: "SortOrder",
                value: 0);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                column: "SortOrder",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ItemTypes");
        }
    }
}
