using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleExpenseAPI.DTOs.Document;
using VehicleExpenseAPI.Services;

namespace VehicleExpenseAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(DocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all receipts for the authenticated user with optional vehicle filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? vehicleId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetAll called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting receipts for user: {UserId}, VehicleId: {VehicleId}", userId, vehicleId);
        var receipts = await _documentService.GetAllAsync(userId, vehicleId);
        
        return Ok(receipts);
    }

    /// <summary>
    /// Get a specific receipt by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetById called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting receipt {ReceiptId} for user: {UserId}", id, userId);
        var receipt = await _documentService.GetByIdAsync(id, userId);

        if (receipt == null)
        {
            _logger.LogWarning("Receipt {ReceiptId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Receipt not found" });
        }

        return Ok(receipt);
    }

    /// <summary>
    /// Step 1: Upload file and run OCR extraction
    /// </summary>
    [HttpPost("upload-for-ocr")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UploadForOcr([FromForm] IFormFile file, [FromForm] int vehicleId)
    {    
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("UploadForOcr called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("UploadForOcr called with null or empty file");
            return BadRequest(new { error = "File is required" });
        }

        _logger.LogInformation("Uploading file for OCR - user: {UserId}, vehicle: {VehicleId}, fileName: {FileName}, size: {FileSize} bytes", 
            userId, vehicleId, file.FileName, file.Length);
        
        var (tempFileId, originalFileName, ocrResult) = await _documentService.UploadForOcrAsync(file, vehicleId, userId);

        if (tempFileId == null)
        {
            _logger.LogWarning("Upload for OCR failed - invalid file or vehicle");
            return BadRequest(new { error = "Invalid file or vehicle. Supported file types: JPG, PNG, PDF. Max size: 5MB" });
        }

        _logger.LogInformation("File uploaded successfully for OCR - tempFileId: {TempFileId}", tempFileId);

        return Ok(new
        {
            tempFileId,
            originalFileName,
            ocrResult
        });
    }

    /// <summary>
    /// Step 2: Confirm OCR results and save receipt permanently
    /// </summary>
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmReceipt([FromBody] ConfirmReceiptDto confirmDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("ConfirmReceipt called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Confirming receipt - user: {UserId}, vehicle: {VehicleId}, tempFileId: {TempFileId}", 
            userId, confirmDto.VehicleId, confirmDto.TempFileId);
        
        var receipt = await _documentService.ConfirmReceiptAsync(confirmDto, userId);

        if (receipt == null)
        {
            _logger.LogWarning("Confirm receipt failed - temp file not found or vehicle invalid");
            return BadRequest(new { error = "Failed to confirm receipt. Temp file may have expired or vehicle is invalid." });
        }

        _logger.LogInformation("Receipt {ReceiptId} confirmed and saved successfully for user {UserId}", 
            receipt.Id, userId);

        return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, receipt);
    }

    /// <summary>
    /// Download a receipt file (secure - only file owner can access)
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Download called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Downloading receipt {ReceiptId} for user: {UserId}", id, userId);
        var receipt = await _documentService.GetByIdAsync(id, userId);

        if (receipt == null)
        {
            _logger.LogWarning("Receipt {ReceiptId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Receipt not found" });
        }

        if (!System.IO.File.Exists(receipt.FilePath))
        {
            _logger.LogError("Receipt file not found on disk: {FilePath}", receipt.FilePath);
            return NotFound(new { error = "Receipt file not found on server" });
        }

        var memory = new MemoryStream();
        using (var stream = new FileStream(receipt.FilePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        var contentType = GetContentType(receipt.OriginalFileName);
        return File(memory, contentType, receipt.OriginalFileName);
    }

    /// <summary>
    /// Link a receipt to an expense
    /// </summary>
    [HttpPut("{receiptId}/link/{expenseId}")]
    public async Task<IActionResult> LinkToExpense(int receiptId, int expenseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("LinkToExpense called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Linking receipt {ReceiptId} to expense {ExpenseId} for user: {UserId}", 
            receiptId, expenseId, userId);
        
        var receipt = await _documentService.LinkToExpenseAsync(receiptId, expenseId, userId);

        if (receipt == null)
        {
            _logger.LogWarning("Failed to link receipt {ReceiptId} to expense {ExpenseId} for user {UserId}", 
                receiptId, expenseId, userId);
            return NotFound(new { error = "Receipt or expense not found, or they belong to different vehicles" });
        }

        _logger.LogInformation("Receipt {ReceiptId} linked to expense {ExpenseId} successfully", receiptId, expenseId);
        return Ok(receipt);
    }

    /// <summary>
    /// Delete a receipt and its associated file
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Delete called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Deleting receipt {ReceiptId} for user: {UserId}", id, userId);
        var success = await _documentService.DeleteAsync(id, userId);

        if (!success)
        {
            _logger.LogWarning("Receipt {ReceiptId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Receipt not found" });
        }

        _logger.LogInformation("Receipt {ReceiptId} deleted successfully for user {UserId}", id, userId);
        return NoContent();
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
