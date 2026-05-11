namespace VehicleExpenseAPI.DTOs.Report;

/// <summary>
/// Summary of all vehicles for dashboard overview
/// </summary>
public class VehicleSummaryDto
{
    public int TotalVehicles { get; set; }
    public decimal TotalInvestment { get; set; } // Sum of all purchase prices
    public decimal TotalFuelCost { get; set; }
    public decimal TotalExpensesCost { get; set; }
    public decimal GrandTotalCost { get; set; }

    public List<VehicleSummaryItem> Vehicles { get; set; } = new();
}

public class VehicleSummaryItem
{
    public int VehicleId { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal MonthlyAverage { get; set; }
}
