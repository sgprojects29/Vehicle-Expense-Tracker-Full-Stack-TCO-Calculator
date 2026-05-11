namespace VehicleExpenseAPI.DTOs.Document;

public class ReceiptDto
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? Merchant { get; set; }
    public decimal? ParsedAmount { get; set; }
    public DateOnly? ParsedDate { get; set; }
    public int VehicleId { get; set; }
    public string VehicleMake { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public int? ExpenseId { get; set; }
    public DateTime UploadedAt { get; set; }
}
