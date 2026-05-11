using System.ComponentModel.DataAnnotations;

namespace VehicleExpenseAPI.Models;

public class FuelEntry
{
    public int Id { get; set; }

    [Required]
    public EnergyType EnergyType { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0")]
    public decimal Cost { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Odometer must be 0 or greater")]
    public int? Odometer { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    // Foreign key
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    // Link to corresponding Expense entry
    public int? ExpenseId { get; set; }
    public Expense? Expense { get; set; }
}
