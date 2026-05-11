using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleExpenseAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddHyridVehicleSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Liters",
                table: "FuelEntries",
                newName: "Amount");

            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EnergyType",
                table: "FuelEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "EnergyType",
                table: "FuelEntries");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "FuelEntries",
                newName: "Liters");
        }
    }
}
