using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddItemTypeIronPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IronPrice",
                table: "ItemTypes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-111111111111"),
                column: "IronPrice",
                value: null);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                column: "IronPrice",
                value: null);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                column: "IronPrice",
                value: null);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                column: "IronPrice",
                value: null);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                column: "IronPrice",
                value: null);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                column: "IronPrice",
                value: null);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                column: "IronPrice",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IronPrice",
                table: "ItemTypes");
        }
    }
}
