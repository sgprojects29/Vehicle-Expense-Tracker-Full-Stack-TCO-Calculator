namespace VehicleExpenseAPI.DTOs.Report;

/// <summary>
/// Total Cost of Ownership report for a vehicle
/// </summary>
public class TcoReportDto
{
    public int VehicleId { get; set; }
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public int VehicleYear { get; set; }

    // Purchase details
    public decimal PurchasePrice { get; set; }
    public DateOnly OwnershipStart { get; set; }
    public DateOnly? OwnershipEnd { get; set; }
    public int OwnershipDays { get; set; }

    // Cost breakdowns
    public decimal TotalFuelCost { get; set; }
    public decimal TotalExpensesCost { get; set; }
    public decimal TotalCost { get; set; } // Purchase + Fuel + Expenses

    // Per-category expenses (mapped from ExpenseCategory enum)
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = [];

    // Per-kilometer calculations
    public decimal? TotalKilometers { get; set; }
    public decimal? CostPerKilometer { get; set; }
    public decimal? FuelCostPerKilometer { get; set; }
    public decimal? ExpensesCostPerKilometer { get; set; }

    // Time-based calculations
    public decimal CostPerDay { get; set; }
    public decimal CostPerMonth { get; set; }

    // Entry counts
    public int TotalFuelEntries { get; set; }
    public int TotalExpenseEntries { get; set; }
}
