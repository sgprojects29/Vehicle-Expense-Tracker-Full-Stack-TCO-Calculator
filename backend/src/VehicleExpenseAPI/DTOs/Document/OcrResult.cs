namespace VehicleExpenseAPI.DTOs.Document;

public class OcrResultDto
{
    public string? Merchant { get; set; }
    public decimal? Amount { get; set; }
    public DateOnly? Date { get; set; }
    public bool IsStubbed { get; set; } = true; // Flag to indicate OCR is stubbed
    public string Message { get; set; } = "OCR functionality is currently stubbed. Please verify extracted data.";
}
