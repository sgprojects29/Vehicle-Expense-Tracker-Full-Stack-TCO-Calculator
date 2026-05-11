using Microsoft.EntityFrameworkCore;
using VehicleExpenseAPI.Data;
using VehicleExpenseAPI.DTOs.Document;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Services;

public class DocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly ReceiptOcrService _ocrService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        ApplicationDbContext context, 
        ReceiptOcrService ocrService, 
        IWebHostEnvironment environment,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _ocrService = ocrService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Get all receipts for a user with optional vehicle filtering
    /// </summary>
    public async Task<IEnumerable<ReceiptDto>> GetAllAsync(string userId, int? vehicleId = null)
    {
        var query = _context.Receipts
            .Include(r => r.Vehicle)
            .Where(r => r.Vehicle!.UserId == userId);

        if (vehicleId.HasValue)
        {
            query = query.Where(r => r.VehicleId == vehicleId.Value);
        }

        var receipts = await query
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync();

        return receipts.Select(MapToDto);
    }

    /// <summary>
    /// Get a specific receipt by ID
    /// </summary>
    public async Task<ReceiptDto?> GetByIdAsync(int id, string userId)
    {
        var receipt = await _context.Receipts
            .Include(r => r.Vehicle)
            .Where(r => r.Id == id && r.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        return receipt == null ? null : MapToDto(receipt);
    }

    /// <summary>
    /// Step 1: Upload file to temp location and run OCR
    /// </summary>
    public async Task<(string? TempFileId, string? OriginalFileName, OcrResultDto? OcrResult)> UploadForOcrAsync(
        IFormFile file,
        int vehicleId, 
        string userId)
    {
        const long maxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        // Verify vehicle belongs to user
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == vehicleId && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            _logger.LogWarning("Upload for OCR failed - Vehicle {VehicleId} not found for user {UserId}", 
                vehicleId, userId);
            return (null, null, null);
        }

        // Validate file size
        if (file.Length > maxFileSizeBytes)
        {
            _logger.LogWarning("Upload for OCR failed - File size {FileSize} exceeds limit", file.Length);
            return (null, null, null);
        }

        // Validate file is not empty
        if (file.Length == 0)
        {
            _logger.LogWarning("Upload for OCR failed - File is empty");
            return (null, null, null);
        }

        // Validate file type
        if (!_ocrService.IsValidImageFile(file))
        {
            _logger.LogWarning("Upload for OCR failed - Invalid file type: {FileName}", file.FileName);
            return (null, null, null);
        }

        // Create temp directory
        var tempPath = Path.Combine(_environment.ContentRootPath, "uploads", "temp");
        Directory.CreateDirectory(tempPath);

        // Generate temp file ID and save file
        var tempFileId = Guid.NewGuid().ToString();
        var fileExtension = Path.GetExtension(file.FileName);
        var tempFilePath = Path.Combine(tempPath, $"{tempFileId}{fileExtension}");

        try
        {
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Temp file saved: {TempFilePath} for user {UserId}", 
                tempFilePath, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save temp file for user {UserId}", userId);
            return (null, null, null);
        }

        // Run OCR
        OcrResultDto? ocrResult = null;
        try
        {
            ocrResult = await _ocrService.ExtractDataAsync(tempFilePath);
            _logger.LogInformation(
                "OCR completed for temp file {TempFileId}: Merchant={Merchant}, Amount={Amount}, Date={Date}", 
                tempFileId, ocrResult.Merchant, ocrResult.Amount, ocrResult.Date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR extraction failed for temp file {TempFileId}", tempFileId);
            // Return temp file ID even if OCR fails - user can still manually enter data
            ocrResult = new OcrResultDto
            {
                Merchant = null,
                Amount = null,
                Date = null,
                IsStubbed = true,
                Message = "OCR extraction failed. Please enter receipt details manually."
            };
        }

        return (tempFileId, file.FileName, ocrResult);
    }

    /// <summary>
    /// Step 2: Confirm OCR results and save receipt permanently
    /// </summary>
    public async Task<ReceiptDto?> ConfirmReceiptAsync(
        ConfirmReceiptDto confirmDto,
        string userId)
    {
        // Verify vehicle belongs to user
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == confirmDto.VehicleId && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            _logger.LogWarning("Confirm receipt failed - Vehicle {VehicleId} not found for user {UserId}", 
                confirmDto.VehicleId, userId);
            return null;
        }

        // Find temp file
        var tempPath = Path.Combine(_environment.ContentRootPath, "uploads", "temp");
        var tempFiles = Directory.GetFiles(tempPath, $"{confirmDto.TempFileId}.*");
        
        if (tempFiles.Length == 0)
        {
            _logger.LogWarning("Confirm receipt failed - Temp file not found: {TempFileId}", 
                confirmDto.TempFileId);
            return null;
        }

        var tempFilePath = tempFiles[0];
        var fileExtension = Path.GetExtension(tempFilePath);

        // Create permanent directory (user-specific and vehicle-specific)
        var permanentPath = Path.Combine(
            _environment.ContentRootPath,
            "uploads",
            "receipts",
            userId,
            confirmDto.VehicleId.ToString()
        );
        Directory.CreateDirectory(permanentPath);

        // Generate unique filename for permanent storage
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var permanentFilePath = Path.Combine(permanentPath, uniqueFileName);

        // Move file from temp to permanent location
        try
        {
            File.Move(tempFilePath, permanentFilePath);
            _logger.LogInformation("File moved from temp to permanent: {PermanentFilePath}", 
                permanentFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move file from temp to permanent location");
            return null;
        }

        // Create receipt record with USER-CONFIRMED data
        var receipt = new Receipt
        {
            FilePath = permanentFilePath,
            OriginalFileName = confirmDto.OriginalFileName ?? $"receipt{fileExtension}",
            Merchant = confirmDto.Merchant,
            ParsedAmount = confirmDto.Amount,
            ParsedDate = confirmDto.Date,
            VehicleId = confirmDto.VehicleId,
            Vehicle = vehicle,
            UploadedAt = DateTime.UtcNow
        };

        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Receipt {ReceiptId} confirmed and saved for user {UserId}", 
            receipt.Id, userId);

        return MapToDto(receipt);
    }

    /// <summary>
    /// Delete a receipt and its associated file
    /// </summary>
    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var receipt = await _context.Receipts
            .Include(r => r.Vehicle)
            .Where(r => r.Id == id && r.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (receipt == null)
        {
            _logger.LogWarning("Delete failed - Receipt {ReceiptId} not found for user {UserId}", 
                id, userId);
            return false;
        }

        // Delete physical file
        if (File.Exists(receipt.FilePath))
        {
            try
            {
                File.Delete(receipt.FilePath);
                _logger.LogInformation("Deleted receipt file: {FilePath}", receipt.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete receipt file: {FilePath}", receipt.FilePath);
                // Continue with database deletion even if file deletion fails
            }
        }
        else
        {
            _logger.LogWarning("Receipt file not found on disk: {FilePath}", receipt.FilePath);
        }

        _context.Receipts.Remove(receipt);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Receipt {ReceiptId} deleted successfully for user {UserId}", 
            id, userId);

        return true;
    }

    /// <summary>
    /// Link a receipt to an expense
    /// </summary>
    public async Task<ReceiptDto?> LinkToExpenseAsync(int receiptId, int expenseId, string userId)
    {
        var receipt = await _context.Receipts
            .Include(r => r.Vehicle)
            .Where(r => r.Id == receiptId && r.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (receipt == null)
        {
            _logger.LogWarning("Link failed - Receipt {ReceiptId} not found for user {UserId}", 
                receiptId, userId);
            return null;
        }

        // Verify expense belongs to user
        var expense = await _context.Expenses
            .Include(e => e.Vehicle)
            .Where(e => e.Id == expenseId && e.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (expense == null)
        {
            _logger.LogWarning("Link failed - Expense {ExpenseId} not found for user {UserId}", 
                expenseId, userId);
            return null;
        }

        // Verify receipt and expense belong to same vehicle
        if (receipt.VehicleId != expense.VehicleId)
        {
            _logger.LogWarning(
                "Link failed - Receipt {ReceiptId} (Vehicle {ReceiptVehicleId}) and " +
                "Expense {ExpenseId} (Vehicle {ExpenseVehicleId}) belong to different vehicles",
                receiptId, receipt.VehicleId, expenseId, expense.VehicleId);
            return null;
        }

        receipt.ExpenseId = expenseId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Receipt {ReceiptId} linked to Expense {ExpenseId} for user {UserId}", 
            receiptId, expenseId, userId);

        return MapToDto(receipt);
    }

    private static ReceiptDto MapToDto(Receipt receipt)
    {
        return new ReceiptDto
        {
            Id = receipt.Id,
            FilePath = receipt.FilePath,
            OriginalFileName = receipt.OriginalFileName,
            Merchant = receipt.Merchant,
            ParsedAmount = receipt.ParsedAmount,
            ParsedDate = receipt.ParsedDate,
            VehicleId = receipt.VehicleId,
            VehicleMake = receipt.Vehicle!.Make,
            VehicleModel = receipt.Vehicle!.Model,
            ExpenseId = receipt.ExpenseId,
            UploadedAt = receipt.UploadedAt
        };
    }
}