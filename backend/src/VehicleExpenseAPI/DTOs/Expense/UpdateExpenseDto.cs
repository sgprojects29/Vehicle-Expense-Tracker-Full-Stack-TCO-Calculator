namespace VehicleExpenseAPI.DTOs.Expense;

public class UpdateExpenseDto
{
    public int Category { get; set; }  // ExpenseCategory enum as int
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public int VehicleId { get; set; }
}
