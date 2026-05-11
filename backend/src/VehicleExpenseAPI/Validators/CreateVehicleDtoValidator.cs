using FluentValidation;
using VehicleExpenseAPI.DTOs.Vehicle;

namespace VehicleExpenseAPI.Validators;

public class CreateVehicleDtoValidator : AbstractValidator<CreateVehicleDto>
{
    public CreateVehicleDtoValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(100).WithMessage("Make cannot exceed 100 characters");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(100).WithMessage("Model cannot exceed 100 characters");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, 2100).WithMessage("Year must be between 1900 and 2100");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price must be zero or positive");

        RuleFor(x => x.OwnershipStart)
            .NotEmpty().WithMessage("Ownership start date is required");

        RuleFor(x => x.OwnershipEnd)
            .GreaterThan(x => x.OwnershipStart)
            .When(x => x.OwnershipEnd.HasValue)
            .WithMessage("Ownership end date must be after start date");
    }
}
