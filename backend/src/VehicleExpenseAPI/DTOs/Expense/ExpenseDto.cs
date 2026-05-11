namespace VehicleExpenseAPI.DTOs.Expense;

public class ExpenseDto
{
    public int Id { get; set; }
    public int Category { get; set; }  // ExpenseCategory enum as int
    public string CategoryName { get; set; } = string.Empty;  // Human-readable name
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public int VehicleId { get; set; }
    
    // Include common vehicle details to avoid additional API calls
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
}
