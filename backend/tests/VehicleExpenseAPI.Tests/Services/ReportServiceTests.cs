using FluentAssertions;
using VehicleExpenseAPI.Data;
using VehicleExpenseAPI.DTOs.Fuel;
using VehicleExpenseAPI.Models;
using VehicleExpenseAPI.Services;
using VehicleExpenseAPI.Tests.Fixtures;
using Xunit;

namespace VehicleExpenseAPI.Tests.Services;

/// <summary>
/// Tests for ReportService focusing on:
/// 1. Fuel cost deduplication in GetCostBreakdownAsync
/// 2. Correct category breakdowns
/// 3. Accurate TCO calculations
/// </summary>
public class ReportServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ReportService _reportService;
    private readonly FuelService _fuelService;
    private readonly string _testUserId = "test-user-456";
    private readonly TestDataBuilder _builder;

    public ReportServiceTests()
    {
        _context = TestDbContextFactory.Create();
        TestDbContextFactory.SeedTestUser(_context, _testUserId);
        _reportService = new ReportService(_context);
        _fuelService = new FuelService(_context);
        _builder = new TestDataBuilder(_testUserId);
    }

    [Fact]
    public async Task GetCostBreakdownAsync_ShouldNotDuplicateFuelCosts()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var createFuelDto = new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 40m,
            Cost = 100m,
            Date = new DateOnly(2025, 12, 1),
            VehicleId = vehicle.Id
        };
        await _fuelService.CreateAsync(createFuelDto, _testUserId);

        var maintenanceExpense = _builder.Expense()
            .ForVehicle(vehicle.Id)
            .WithCategory(ExpenseCategory.Maintenance)
            .WithAmount(200m)
            .WithDate(new DateOnly(2025, 12, 5))
            .Build();
        _context.Expenses.Add(maintenanceExpense);
        await _context.SaveChangesAsync();

        var result = await _reportService.GetCostBreakdownAsync(vehicle.Id, _testUserId);

        result.Should().NotBeNull();

        // Fuel should be a separate category with $100
        var fuelCategory = result!.CategoryBreakdown.FirstOrDefault(c => c.Category == "Fuel & Charging");
        fuelCategory.Should().NotBeNull();
        fuelCategory!.Amount.Should().Be(100m);
        fuelCategory.Count.Should().Be(1);

        // Maintenance should only have $200
        var maintenanceCategory = result.CategoryBreakdown.FirstOrDefault(c => c.Category == "Maintenance");
        maintenanceCategory.Should().NotBeNull();
        maintenanceCategory!.Amount.Should().Be(200m);
        maintenanceCategory.Count.Should().Be(1);

        // Other category should NOT exist (fuel-linked expense was excluded)
        var otherCategory = result.CategoryBreakdown.FirstOrDefault(c => c.Category == "Other");
        otherCategory.Should().BeNull();

        result.TotalFuelCost.Should().Be(100m);

        result.TotalExpensesCost.Should().Be(200m);

        result.TotalCost.Should().Be(vehicle.PurchasePrice + 300m);
    }

    [Fact]
    public async Task GetCostBreakdownAsync_WithMultipleFuelEntries_ShouldAggregateCorrectly()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Create 3 fuel entries
        for (int i = 1; i <= 3; i++)
        {
            var createFuelDto = new CreateFuelEntryDto
            {
                EnergyType = EnergyType.Gasoline,
                Amount = 40m,
                Cost = 50m * i,
                Date = new DateOnly(2025, 12, i),
                VehicleId = vehicle.Id
            };
            await _fuelService.CreateAsync(createFuelDto, _testUserId);
        }

        var result = await _reportService.GetCostBreakdownAsync(vehicle.Id, _testUserId);

        var fuelCategory = result!.CategoryBreakdown.FirstOrDefault(c => c.Category == "Fuel & Charging");
        fuelCategory.Should().NotBeNull();

        // Total fuel cost: $50 + $100 + $150 = $300
        fuelCategory!.Amount.Should().Be(300m);
        fuelCategory.Count.Should().Be(3);

        result.TotalFuelCost.Should().Be(300m);

        // No expenses should be counted (all are fuel-linked)
        result.TotalExpensesCost.Should().Be(0m);
    }

    [Fact]
    public async Task GetCostBreakdownAsync_WithMixedExpenses_ShouldSeparateCorrectly()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        await _fuelService.CreateAsync(new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 40m,
            Cost = 75m,
            Date = new DateOnly(2025, 12, 1),
            VehicleId = vehicle.Id
        }, _testUserId);

        await _fuelService.CreateAsync(new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Diesel,
            Amount = 50m,
            Cost = 85m,
            Date = new DateOnly(2025, 12, 2),
            VehicleId = vehicle.Id
        }, _testUserId);

        _context.Expenses.AddRange(
            _builder.Expense()
                .ForVehicle(vehicle.Id)
                .WithCategory(ExpenseCategory.Maintenance)
                .WithAmount(150m)
                .Build(),
            _builder.Expense()
                .ForVehicle(vehicle.Id)
                .WithCategory(ExpenseCategory.Insurance)
                .WithAmount(500m)
                .Build(),
            _builder.Expense()
                .ForVehicle(vehicle.Id)
                .WithCategory(ExpenseCategory.Maintenance)
                .WithAmount(100m)
                .Build()
        );
        await _context.SaveChangesAsync();

        var result = await _reportService.GetCostBreakdownAsync(vehicle.Id, _testUserId);

        result.Should().NotBeNull();

        // Fuel: $75 + $85 = $160
        var fuelCategory = result!.CategoryBreakdown.FirstOrDefault(c => c.Category == "Fuel & Charging");
        fuelCategory!.Amount.Should().Be(160m);

        // Maintenance: $150 + $100 = $250 (2 entries)
        var maintenanceCategory = result.CategoryBreakdown.FirstOrDefault(c => c.Category == "Maintenance");
        maintenanceCategory!.Amount.Should().Be(250m);
        maintenanceCategory.Count.Should().Be(2);

        // Insurance: $500
        var insuranceCategory = result.CategoryBreakdown.FirstOrDefault(c => c.Category == "Insurance");
        insuranceCategory!.Amount.Should().Be(500m);

        // Total expenses (not including fuel-linked): $750
        result.TotalExpensesCost.Should().Be(750m);
        result.TotalFuelCost.Should().Be(160m);
    }

    [Fact]
    public async Task GetTcoReportAsync_ShouldIncludeFuelAndExpenseSeparately()
    {
        var vehicle = _builder.Vehicle()
            .WithOwnershipStart(new DateOnly(2025, 1, 1))
            .Build();
        vehicle.PurchasePrice = 20000m;
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        await _fuelService.CreateAsync(new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 40m,
            Cost = 150m,
            Odometer = 10000,
            Date = new DateOnly(2025, 6, 1),
            VehicleId = vehicle.Id
        }, _testUserId);

        var maintenanceExpense = _builder.Expense()
            .ForVehicle(vehicle.Id)
            .WithCategory(ExpenseCategory.Maintenance)
            .WithAmount(350m)
            .Build();
        _context.Expenses.Add(maintenanceExpense);
        await _context.SaveChangesAsync();

        var result = await _reportService.GetTcoReportAsync(vehicle.Id, _testUserId);

        result.Should().NotBeNull();
        result!.PurchasePrice.Should().Be(20000m);
        result.TotalFuelCost.Should().Be(150m);

        // NOTE: TCO includes the fuel-linked expense in TotalExpensesCost
        // This is different from CostBreakdown which excludes it
        result.TotalExpensesCost.Should().Be(500m); // $150 (fuel-linked) + $350 (maintenance)

        // TCO: $20,000 + $150 + $500 = $20,650
        result.TotalCost.Should().Be(20650m);
        result.TotalFuelEntries.Should().Be(1);
        result.TotalExpenseEntries.Should().Be(2); // Both expenses counted
    }

    [Fact]
    public async Task GetVehicleSummaryAsync_ShouldCalculateMultipleVehiclesCorrectly()
    {
        var vehicle1 = _builder.Vehicle()
            .WithModel("Camry")
            .Build();
        var vehicle2 = _builder.Vehicle()
            .WithModel("Prius")
            .Build();

        _context.Vehicles.AddRange(vehicle1, vehicle2);
        await _context.SaveChangesAsync();

        await _fuelService.CreateAsync(new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 40m,
            Cost = 100m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            VehicleId = vehicle1.Id
        }, _testUserId);

        await _fuelService.CreateAsync(new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 50m,
            Cost = 200m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            VehicleId = vehicle2.Id
        }, _testUserId);

        var result = await _reportService.GetVehicleSummaryAsync(_testUserId);

        result.Should().NotBeNull();
        result.TotalVehicles.Should().Be(2);
        result.Vehicles.Should().HaveCount(2);

        var camry = result.Vehicles.FirstOrDefault(v => v.Model == "Camry");
        camry.Should().NotBeNull();

        var prius = result.Vehicles.FirstOrDefault(v => v.Model == "Prius");
        prius.Should().NotBeNull();

        // Total fuel includes fuel-linked expenses
        result.TotalFuelCost.Should().Be(300m);
        result.TotalExpensesCost.Should().Be(300m); // Fuel-linked expenses counted here too
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
