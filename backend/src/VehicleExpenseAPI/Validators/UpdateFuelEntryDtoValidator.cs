using FluentValidation;
using VehicleExpenseAPI.DTOs.Fuel;

namespace VehicleExpenseAPI.Validators;

public class UpdateFuelEntryDtoValidator : AbstractValidator<UpdateFuelEntryDto>
{
    public UpdateFuelEntryDtoValidator()
    {
        RuleFor(x => x.EnergyType)
            .IsInEnum()
            .When(x => x.EnergyType.HasValue)
            .WithMessage("Invalid energy type");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .When(x => x.Amount.HasValue)
            .WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Cost)
            .GreaterThan(0)
            .When(x => x.Cost.HasValue)
            .WithMessage("Cost must be greater than 0");

        RuleFor(x => x.Odometer)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Odometer.HasValue)
            .WithMessage("Odometer must be 0 or greater");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.Date.HasValue)
            .WithMessage("Date cannot be in the future");
    }
}
