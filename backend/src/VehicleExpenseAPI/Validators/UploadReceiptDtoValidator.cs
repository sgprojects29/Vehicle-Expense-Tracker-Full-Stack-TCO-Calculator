using FluentValidation;
using VehicleExpenseAPI.DTOs.Document;

namespace VehicleExpenseAPI.Validators;

public class UploadReceiptDtoValidator : AbstractValidator<UploadReceiptDto>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "application/pdf" };

    public UploadReceiptDtoValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0)
            .WithMessage("VehicleId must be greater than 0");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required")
            .Must(file => file.Length > 0)
            .WithMessage("File cannot be empty")
            .Must(file => file.Length <= MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024 / 1024}MB")
            .Must(file => 
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                return AllowedExtensions.Contains(extension);
            })
            .WithMessage($"File must be one of the following types: {string.Join(", ", AllowedExtensions)}")
            .Must(file => AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            .WithMessage("Invalid file content type");

        RuleFor(x => x.Merchant)
            .NotEmpty()
            .WithMessage("Merchant is required")
            .MaximumLength(200)
            .WithMessage("Merchant name cannot exceed 200 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Amount seems unreasonably high");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the future")
            .Must(date => date >= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)))
            .WithMessage("Date cannot be more than 10 years in the past");
    }
}
