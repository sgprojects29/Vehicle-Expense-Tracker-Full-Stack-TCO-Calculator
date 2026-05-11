using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleExpenseAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseIdToFuelEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpenseId",
                table: "FuelEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelEntries_ExpenseId",
                table: "FuelEntries",
                column: "ExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelEntries_Expenses_ExpenseId",
                table: "FuelEntries",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuelEntries_Expenses_ExpenseId",
                table: "FuelEntries");

            migrationBuilder.DropIndex(
                name: "IX_FuelEntries_ExpenseId",
                table: "FuelEntries");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "FuelEntries");
        }
    }
}
