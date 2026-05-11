namespace VehicleExpenseAPI.DTOs.Fuel;

public class FuelEfficiencyDto
{
    public int VehicleId { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalKilometers { get; set; }
    public decimal? AverageLitersPer100Km { get; set; }
    public decimal? AverageKilometersPerLiter { get; set; }
    public decimal? AverageCostPerKilometer { get; set; }
    public decimal AverageCostPerFillUp { get; set; }
    public int TotalFillUps { get; set; }
    public int EntriesWithOdometer { get; set; }
    public int EntriesWithoutOdometer { get; set; }
}
