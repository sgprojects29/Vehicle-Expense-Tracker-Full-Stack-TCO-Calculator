namespace VehicleExpenseAPI.DTOs.Report;

/// <summary>
/// Monthly cost trends for a vehicle
/// </summary>
public class MonthlyCostTrendDto
{
    public int VehicleId { get; set; }
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;

    public List<MonthlyDataPoint> MonthlyData { get; set; } = new();
}

public class MonthlyDataPoint
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal FuelCost { get; set; }
    public decimal ExpensesCost { get; set; }
    public decimal TotalCost { get; set; }
    public int FuelEntries { get; set; }
    public int ExpenseEntries { get; set; }
}
