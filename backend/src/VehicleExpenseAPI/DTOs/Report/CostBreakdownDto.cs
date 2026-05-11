namespace VehicleExpenseAPI.DTOs.Report;

/// <summary>
/// Cost breakdown by category for a vehicle
/// </summary>
public class CostBreakdownDto
{
    public int VehicleId { get; set; }
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;

    public decimal PurchasePrice { get; set; }
    public decimal TotalFuelCost { get; set; }
    public decimal TotalExpensesCost { get; set; }
    public decimal TotalCost { get; set; }

    // Breakdown by expense category
    public List<CategoryBreakdownItem> CategoryBreakdown { get; set; } = new();
}

public class CategoryBreakdownItem
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int Count { get; set; }
}
