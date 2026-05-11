using System.ComponentModel.DataAnnotations;

namespace VehicleExpenseAPI.Models;

public class Receipt
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Merchant { get; set; }
    
    public decimal? ParsedAmount { get; set; }
    
    public DateOnly? ParsedDate { get; set; }
    
    // Foreign key to Vehicle (required - receipts belong to a vehicle)
    [Required]
    public int VehicleId { get; set; }
    
    // Navigation property
    public Vehicle? Vehicle { get; set; }
    
    // Foreign key to Expense (nullable - can upload receipt before creating expense)
    public int? ExpenseId { get; set; }
    
    // Navigation property
    public Expense? Expense { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
