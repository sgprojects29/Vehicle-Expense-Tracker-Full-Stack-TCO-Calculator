using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.DTOs.Fuel;

public class UpdateFuelEntryDto
{
    public EnergyType? EnergyType { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Cost { get; set; }
    public int? Odometer { get; set; } // Already nullable, but now it's truly optional
    public DateOnly? Date { get; set; }
}
