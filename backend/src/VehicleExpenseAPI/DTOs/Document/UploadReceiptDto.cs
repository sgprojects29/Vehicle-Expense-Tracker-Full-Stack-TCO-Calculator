using System.ComponentModel.DataAnnotations;

namespace VehicleExpenseAPI.DTOs.Document;

public class UploadReceiptDto
{
    [Required]
    public int VehicleId { get; set; }
    
    [Required]
    public IFormFile File { get; set; } = null!;
    
    // Optional fields - user can change after if OCR failed or was incorrect
    // These fields will be required in the ConfirmReceiptDto
    [MaxLength(200)]
    public string? Merchant { get; set; }
    
    public decimal? Amount { get; set; }
    
    public DateOnly? Date { get; set; }
}
