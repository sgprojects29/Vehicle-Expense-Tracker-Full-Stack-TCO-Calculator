using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.DTOs.Fuel;

public class CreateFuelEntryDto
{
    public required EnergyType EnergyType { get; set; }
    public required decimal Amount { get; set; }
    public required decimal Cost { get; set; }
    public int? Odometer { get; set; }
    public required DateOnly Date { get; set; }
    public required int VehicleId { get; set; }
}
