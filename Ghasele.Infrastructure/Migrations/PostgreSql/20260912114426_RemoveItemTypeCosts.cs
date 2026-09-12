using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class RemoveItemTypeCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BothCost",
                table: "ItemTypes");

            migrationBuilder.DropColumn(
                name: "CleaningCost",
                table: "ItemTypes");

            migrationBuilder.DropColumn(
                name: "IronCost",
                table: "ItemTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BothCost",
                table: "ItemTypes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CleaningCost",
                table: "ItemTypes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IronCost",
                table: "ItemTypes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-111111111111"),
                columns: new[] { "BothCost", "CleaningCost", "IronCost" },
                values: new object[] { 0.40m, 0.30m, 0.20m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                columns: new[] { "BothCost", "CleaningCost", "IronCost" },
                values: new object[] { 0.50m, 0.40m, 0.30m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                columns: new[] { "BothCost", "CleaningCost", "IronCost" },
                values: new object[] { 2.00m, 1.50m, 1.00m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                columns: new[] { "BothCost", "CleaningCost", "IronCost" },
                values: new object[] { 5.00m, 3.00m, 1.50m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                columns: new[] { "BothCost", "CleaningCost", "IronCost" },
                values: new object[] { 1.00m, 0.80m, 0.60m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                columns: new[] { "BothCost", "CleaningCost", "IronCost" },
                values: new object[] { 2.50m, 2.50m, 0.00m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                columns: new[] { "BothCost", "CleaningCost", "IronCost" },
                values: new object[] { 0.70m, 0.50m, 0.40m });
        }
    }
}
