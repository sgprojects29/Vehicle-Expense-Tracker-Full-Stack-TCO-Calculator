using FluentValidation;
using VehicleExpenseAPI.DTOs.Expense;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Validators;

public class CreateExpenseDtoValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseDtoValidator()
    {
        RuleFor(x => x.Category)
            .InclusiveBetween(0, 9)
            .WithMessage("Invalid expense category. Must be between 0 (Fuel) and 9 (Other)");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now.AddDays(1)))
            .WithMessage("Date cannot be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");

        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("Valid vehicle ID is required");
    }
}
