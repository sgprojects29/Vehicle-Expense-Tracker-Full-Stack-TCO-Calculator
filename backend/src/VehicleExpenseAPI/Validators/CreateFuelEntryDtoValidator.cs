using FluentValidation;
using VehicleExpenseAPI.DTOs.Fuel;

namespace VehicleExpenseAPI.Validators;

public class CreateFuelEntryDtoValidator : AbstractValidator<CreateFuelEntryDto>
{
    public CreateFuelEntryDtoValidator()
    {
        RuleFor(x => x.EnergyType)
            .IsInEnum()
            .WithMessage("Invalid energy type");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Cost)
            .GreaterThan(0)
            .WithMessage("Cost must be greater than 0");

        RuleFor(x => x.Odometer)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Odometer.HasValue)
            .WithMessage("Odometer must be 0 or greater");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the future");

        RuleFor(x => x.VehicleId)
            .GreaterThan(0)
            .WithMessage("Valid vehicle ID is required");
    }
}
