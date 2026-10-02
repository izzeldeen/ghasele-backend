using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ghasele.Infrastructure.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class SingleServicePricingAndCleanerShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The three service prices collapse into one, and the one that survives is
            // BothPrice: washing and ironing together is exactly the service being kept, so
            // it is the only column whose figures are still true.
            //
            // Hand-written. The scaffolder chose to rename IronPrice instead - it cannot know
            // which of three interchangeable columns carries the meaning - which would have
            // left every item priced at its ironing-only rate. A shirt would have gone from
            // 1.00 to 0.50 with nothing in the schema to show it had happened.
            migrationBuilder.DropColumn(
                name: "IronPrice",
                table: "ItemTypes");

            migrationBuilder.DropColumn(
                name: "CleaningPrice",
                table: "ItemTypes");

            migrationBuilder.RenameColumn(
                name: "BothPrice",
                table: "ItemTypes",
                newName: "Price");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPrice",
                table: "ItemTypes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            // 50, not 0: every laundry already on the books is on the standard half share,
            // and a 0 here would record every order they handle as costing us nothing until
            // somebody noticed and typed a number in.
            migrationBuilder.AddColumn<decimal>(
                name: "SharePercentage",
                table: "Cleaners",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 50m);

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-111111111111"),
                columns: new[] { "MaxPrice", "Price" },
                values: new object[] { null, 1.00m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                columns: new[] { "MaxPrice", "Price" },
                values: new object[] { null, 1.25m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                columns: new[] { "MaxPrice", "Price" },
                values: new object[] { null, 5.00m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                columns: new[] { "MaxPrice", "Price" },
                values: new object[] { null, 12.00m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                columns: new[] { "MaxPrice", "Price" },
                values: new object[] { null, 2.50m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                columns: new[] { "MaxPrice", "Price" },
                values: new object[] { null, 6.00m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                columns: new[] { "MaxPrice", "Price" },
                values: new object[] { null, 1.75m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "ItemTypes");

            migrationBuilder.DropColumn(
                name: "SharePercentage",
                table: "Cleaners");

            // Mirrors the hand-written Up: Price goes back to being BothPrice, and the two
            // dropped columns come back empty. Their old figures are gone - Up dropped them -
            // so a rollback restores the shape of the schema, not the prices that were in it.
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "ItemTypes",
                newName: "BothPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "IronPrice",
                table: "ItemTypes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CleaningPrice",
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
                columns: new[] { "BothPrice", "CleaningPrice", "IronPrice" },
                values: new object[] { 1.00m, 0.75m, 0.50m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-222222222222"),
                columns: new[] { "BothPrice", "CleaningPrice", "IronPrice" },
                values: new object[] { 1.25m, 1.00m, 0.75m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-333333333333"),
                columns: new[] { "BothPrice", "CleaningPrice", "IronPrice" },
                values: new object[] { 5.00m, 3.50m, 2.50m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-444444444444"),
                columns: new[] { "BothPrice", "CleaningPrice", "IronPrice" },
                values: new object[] { 12.00m, 8.00m, 4.00m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-555555555555"),
                columns: new[] { "BothPrice", "CleaningPrice", "IronPrice" },
                values: new object[] { 2.50m, 2.00m, 1.50m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-666666666666"),
                columns: new[] { "BothPrice", "CleaningPrice", "IronPrice" },
                values: new object[] { 6.00m, 6.00m, 0.00m });

            migrationBuilder.UpdateData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: new Guid("f9e1e1e1-1234-4a5b-bcde-777777777777"),
                columns: new[] { "BothPrice", "CleaningPrice", "IronPrice" },
                values: new object[] { 1.75m, 1.25m, 1.00m });
        }
    }
}
