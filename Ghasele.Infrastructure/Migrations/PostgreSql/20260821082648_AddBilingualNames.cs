using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddBilingualNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "ItemTypes",
                newName: "TypeNameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Drivers",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Cleaners",
                newName: "NameEn");

            migrationBuilder.AddColumn<string>(
                name: "TypeNameAr",
                table: "ItemTypes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Drivers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Cleaners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-111111111111"),
                columns: new[] { "TypeNameAr", "TypeNameEn" },
                values: new object[] { "قميص", "Shirt" });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                columns: new[] { "TypeNameAr", "TypeNameEn" },
                values: new object[] { "بنطلون", "Trousers" });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                columns: new[] { "TypeNameAr", "TypeNameEn" },
                values: new object[] { "بدلة رجالية", "Men's Suit" });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                columns: new[] { "TypeNameAr", "TypeNameEn" },
                values: new object[] { "فستان سهرة", "Evening Dress" });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                columns: new[] { "TypeNameAr", "TypeNameEn" },
                values: new object[] { "جاكيت", "Jacket" });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                columns: new[] { "TypeNameAr", "TypeNameEn" },
                values: new object[] { "لحاف/بطانية كبير", "Large Blanket" });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                columns: new[] { "TypeNameAr", "TypeNameEn" },
                values: new object[] { "ثوب/دشداشة", "Thobe" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeNameAr",
                table: "ItemTypes");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Cleaners");

            migrationBuilder.RenameColumn(
                name: "TypeNameEn",
                table: "ItemTypes",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Drivers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Cleaners",
                newName: "Name");

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-111111111111"),
                column: "TypeName",
                value: "قميص");

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                column: "TypeName",
                value: "بنطلون");

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                column: "TypeName",
                value: "بدلة رجالية");

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                column: "TypeName",
                value: "فستان سهرة");

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                column: "TypeName",
                value: "جاكيت");

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                column: "TypeName",
                value: "لحاف/بطانية كبير");

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                column: "TypeName",
                value: "ثوب/دشداشة");
        }
    }
}
