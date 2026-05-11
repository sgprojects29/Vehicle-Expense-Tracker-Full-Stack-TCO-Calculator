using VehicleExpenseAPI.DTOs.Document;

namespace VehicleExpenseAPI.Services;

/// <summary>
/// OCR service for extracting data from receipt images
/// Currently stubbed - returns placeholder data
/// </summary>
public class ReceiptOcrService
{
    private readonly ILogger<ReceiptOcrService> _logger;

    public ReceiptOcrService(ILogger<ReceiptOcrService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract merchant, amount, and date from receipt image (STUBBED)
    /// In production, this would use Tesseract, AWS Textract, or similar OCR service
    /// </summary>
    public async Task<OcrResultDto> ExtractDataAsync(string filePath)
    {
        _logger.LogInformation("OCR extraction requested for file: {FilePath} (using stub)", filePath);

        // Simulate async processing delay
        await Task.Delay(500);

        // Return stubbed data
        return new OcrResultDto
        {
            Merchant = "Sample Merchant",
            Amount = 50.00m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            IsStubbed = true,
            Message = "OCR is currently stubbed. Please manually verify and update the extracted data."
        };
    }

    /// <summary>
    /// Validate that the file is a supported image format
    /// </summary>
    public bool IsValidImageFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
        
        var validMimeTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
        
        return validExtensions.Contains(extension) && 
               validMimeTypes.Contains(file.ContentType.ToLowerInvariant());
    }
}
