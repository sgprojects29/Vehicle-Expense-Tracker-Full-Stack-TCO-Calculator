using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Tests.Fixtures;

/// <summary>
/// Fluent builder for creating test data with realistic defaults.
/// </summary>
public class TestDataBuilder
{
    private readonly string _userId;

    public TestDataBuilder(string userId = "test-user-id")
    {
        _userId = userId;
    }

    public VehicleBuilder Vehicle() => new VehicleBuilder(_userId);
    public FuelEntryBuilder FuelEntry() => new FuelEntryBuilder();
    public ExpenseBuilder Expense() => new ExpenseBuilder();
}

public class VehicleBuilder
{
    private readonly Vehicle _vehicle;

    public VehicleBuilder(string userId = "test-user-id")
    {
        _vehicle = new Vehicle
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2020,
            PurchasePrice = 25000m,
            OwnershipStart = new DateOnly(2020, 1, 1),
            VehicleType = VehicleType.Gasoline,
            UserId = userId
        };
    }

    public VehicleBuilder WithMake(string make)
    {
        _vehicle.Make = make;
        return this;
    }

    public VehicleBuilder WithModel(string model)
    {
        _vehicle.Model = model;
        return this;
    }

    public VehicleBuilder WithYear(int year)
    {
        _vehicle.Year = year;
        return this;
    }

    public VehicleBuilder WithType(VehicleType type)
    {
        _vehicle.VehicleType = type;
        return this;
    }

    public VehicleBuilder WithOwnershipStart(DateOnly date)
    {
        _vehicle.OwnershipStart = date;
        return this;
    }

    public VehicleBuilder Electric()
    {
        _vehicle.VehicleType = VehicleType.Electric;
        return this;
    }

    public Vehicle Build() => _vehicle;
}

public class FuelEntryBuilder
{
    private readonly FuelEntry _fuelEntry;

    public FuelEntryBuilder()
    {
        _fuelEntry = new FuelEntry
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 40.5m,
            Cost = 60.75m,
            Odometer = 10000,
            Date = DateOnly.FromDateTime(DateTime.Today)
        };
    }

    public FuelEntryBuilder ForVehicle(int vehicleId)
    {
        _fuelEntry.VehicleId = vehicleId;
        return this;
    }

    public FuelEntryBuilder WithEnergyType(EnergyType type)
    {
        _fuelEntry.EnergyType = type;
        return this;
    }

    public FuelEntryBuilder WithAmount(decimal amount)
    {
        _fuelEntry.Amount = amount;
        return this;
    }

    public FuelEntryBuilder WithCost(decimal cost)
    {
        _fuelEntry.Cost = cost;
        return this;
    }

    public FuelEntryBuilder WithOdometer(int? odometer)
    {
        _fuelEntry.Odometer = odometer;
        return this;
    }

    public FuelEntryBuilder WithoutOdometer()
    {
        _fuelEntry.Odometer = null;
        return this;
    }

    public FuelEntryBuilder WithDate(DateOnly date)
    {
        _fuelEntry.Date = date;
        return this;
    }

    public FuelEntryBuilder WithLinkedExpense(int expenseId)
    {
        _fuelEntry.ExpenseId = expenseId;
        return this;
    }

    public FuelEntry Build() => _fuelEntry;
}

public class ExpenseBuilder
{
    private readonly Expense _expense;

    public ExpenseBuilder()
    {
        _expense = new Expense
        {
            Category = ExpenseCategory.Maintenance,
            Amount = 100m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Notes = null
        };
    }

    public ExpenseBuilder ForVehicle(int vehicleId)
    {
        _expense.VehicleId = vehicleId;
        return this;
    }

    public ExpenseBuilder WithCategory(ExpenseCategory category)
    {
        _expense.Category = category;
        return this;
    }

    public ExpenseBuilder WithAmount(decimal amount)
    {
        _expense.Amount = amount;
        return this;
    }

    public ExpenseBuilder WithDate(DateOnly date)
    {
        _expense.Date = date;
        return this;
    }

    public ExpenseBuilder WithNotes(string notes)
    {
        _expense.Notes = notes;
        return this;
    }

    public Expense Build() => _expense;
}
