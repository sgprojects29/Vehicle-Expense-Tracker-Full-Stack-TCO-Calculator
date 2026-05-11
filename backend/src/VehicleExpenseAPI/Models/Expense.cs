using System.ComponentModel.DataAnnotations;

namespace VehicleExpenseAPI.Models;

public class Expense
{
    public int Id { get; set; }
    
    [Required]
    public ExpenseCategory Category { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [Required]
    public DateOnly Date { get; set; }  // Changed from DateTime to DateOnly
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Foreign key to Vehicle
    [Required]
    public int VehicleId { get; set; }
    
    // Navigation property
    public Vehicle? Vehicle { get; set; }
}
