using System.ComponentModel.DataAnnotations;

namespace VehicleExpenseAPI.Models;

public class Vehicle
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Make { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;
    
    [Range(1900, 2100)]
    public int Year { get; set; }
    
    public decimal PurchasePrice { get; set; }
    
    public DateOnly OwnershipStart { get; set; }
    
    public DateOnly? OwnershipEnd { get; set; }
    
    public VehicleType VehicleType { get; set; } = VehicleType.Gasoline;
    
    // Foreign key to User
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    // Navigation property
    public User? User { get; set; }
}
