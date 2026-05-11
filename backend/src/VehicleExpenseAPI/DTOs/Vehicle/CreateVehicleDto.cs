using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.DTOs.Vehicle;

public class CreateVehicleDto
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal PurchasePrice { get; set; }
    public DateOnly OwnershipStart { get; set; }
    public DateOnly? OwnershipEnd { get; set; }
    public VehicleType VehicleType { get; set; } = VehicleType.Gasoline;
}
