using System.ComponentModel.DataAnnotations;

namespace VehicleExpenseAPI.DTOs.Document;

public class ConfirmReceiptDto
{
    [Required]
    public int VehicleId { get; set; }
    
    [Required]
    public string TempFileId { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? OriginalFileName { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Merchant { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [Required]
    public DateOnly Date { get; set; }
}
