using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleExpenseAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleIdToReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "ParsedDate",
                table: "Receipts",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Receipts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "Receipts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_VehicleId",
                table: "Receipts",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Vehicles_VehicleId",
                table: "Receipts",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Vehicles_VehicleId",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_VehicleId",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "Receipts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ParsedDate",
                table: "Receipts",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
