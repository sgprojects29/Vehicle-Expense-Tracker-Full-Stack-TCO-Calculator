using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.DTOs.Fuel;

public class FuelEntryDto
{
    public int Id { get; set; }
    public EnergyType EnergyType { get; set; }
    public string EnergyTypeDisplay { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Unit { get; set; } = string.Empty;  // "L" or "kWh"
    public decimal Cost { get; set; }
    public int? Odometer { get; set; }
    public DateOnly Date { get; set; }
    public int VehicleId { get; set; }
    
    // Include vehicle details to reduce API calls
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    
    // Calculated fields
    public decimal CostPerUnit { get; set; }  
}
