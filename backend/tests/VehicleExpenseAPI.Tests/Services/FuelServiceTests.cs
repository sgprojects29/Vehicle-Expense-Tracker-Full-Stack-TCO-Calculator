using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleExpenseAPI.Data;
using VehicleExpenseAPI.DTOs.Fuel;
using VehicleExpenseAPI.Models;
using VehicleExpenseAPI.Services;
using VehicleExpenseAPI.Tests.Fixtures;
using Xunit;

namespace VehicleExpenseAPI.Tests.Services;

/// <summary>
/// Tests for FuelService focusing on:
/// 1. Optional odometer functionality
/// 2. Fuel efficiency calculations with missing odometer data
/// 3. Linked expense creation and deletion
/// </summary>
public class FuelServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FuelService _fuelService;
    private readonly string _testUserId = "test-user-123";
    private readonly TestDataBuilder _builder;

    public FuelServiceTests()
    {
        _context = TestDbContextFactory.Create();
        TestDbContextFactory.SeedTestUser(_context, _testUserId);
        _fuelService = new FuelService(_context);
        _builder = new TestDataBuilder(_testUserId);
    }

    [Fact]
    public async Task CreateAsync_WithOdometer_ShouldCreateFuelEntryAndLinkedExpense()
    {
        // Arrange
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var createDto = new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 45.5m,
            Cost = 68.25m,
            Odometer = 15000,
            Date = new DateOnly(2025, 12, 15),
            VehicleId = vehicle.Id
        };

        var result = await _fuelService.CreateAsync(createDto, _testUserId);

        result.Should().NotBeNull();
        result!.EnergyType.Should().Be(EnergyType.Gasoline);
        result.EnergyTypeDisplay.Should().Be("Gasoline");
        result.Amount.Should().Be(45.5m);
        result.Cost.Should().Be(68.25m);
        result.Odometer.Should().Be(15000);
        result.CostPerUnit.Should().BeApproximately(1.5m, 0.01m); // 68.25 / 45.5 ≈ 1.50

        var fuelEntry = await _context.FuelEntries
            .Include(f => f.Expense)
            .FirstAsync(f => f.Id == result.Id);

        fuelEntry.ExpenseId.Should().NotBeNull();
        fuelEntry.Expense.Should().NotBeNull();
        fuelEntry.Expense!.Amount.Should().Be(68.25m);
    }

    [Fact]
    public async Task CreateAsync_WithoutOdometer_ShouldCreateSuccessfully()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var createDto = new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Diesel,
            Amount = 50.0m,
            Cost = 85.00m,
            Odometer = null, // No odometer reading
            Date = new DateOnly(2025, 12, 20),
            VehicleId = vehicle.Id
        };

        var result = await _fuelService.CreateAsync(createDto, _testUserId);

        result.Should().NotBeNull();
        result!.Odometer.Should().BeNull();
        result.Amount.Should().Be(50.0m);
        result.Cost.Should().Be(85.00m);
    }

    [Fact]
    public async Task UpdateAsync_AllowsOdometerToBeUpdated()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var fuelEntry = _builder.FuelEntry()
            .ForVehicle(vehicle.Id)
            .WithOdometer(12000)
            .Build();
        _context.FuelEntries.Add(fuelEntry);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateFuelEntryDto
        {
            Odometer = 12500 // Update odometer
        };

        var (success, result) = await _fuelService.UpdateAsync(fuelEntry.Id, updateDto, _testUserId);

        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Odometer.Should().Be(12500);
    }

    [Fact]
    public async Task UpdateAsync_SyncsCostWithLinkedExpense()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var createDto = new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 40m,
            Cost = 60m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            VehicleId = vehicle.Id
        };

        var created = await _fuelService.CreateAsync(createDto, _testUserId);

        var updateDto = new UpdateFuelEntryDto
        {
            Cost = 75m // Update cost
        };

        var (success, result) = await _fuelService.UpdateAsync(created!.Id, updateDto, _testUserId);

        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Cost.Should().Be(75m);

        var fuelEntry = await _context.FuelEntries
            .Include(f => f.Expense)
            .FirstAsync(f => f.Id == created.Id);

        fuelEntry.Expense.Should().NotBeNull();
        fuelEntry.Expense!.Amount.Should().Be(75m);
    }

    [Fact]
    public async Task GetEfficiencyAsync_WithMixedOdometerData_ShouldCalculateCorrectly()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // Entry 1: Odometer 10000, 40L, $60
        var entry1 = _builder.FuelEntry()
            .ForVehicle(vehicle.Id)
            .WithOdometer(10000)
            .WithAmount(40m)
            .WithCost(60m)
            .WithDate(new DateOnly(2025, 11, 1))
            .Build();

        // Entry 2: Odometer 10500, 35L, $52.5 (500 km driven)
        var entry2 = _builder.FuelEntry()
            .ForVehicle(vehicle.Id)
            .WithOdometer(10500)
            .WithAmount(35m)
            .WithCost(52.5m)
            .WithDate(new DateOnly(2025, 11, 15))
            .Build();

        // Entry 3: No odometer, 45L, $67.5 (should be excluded from efficiency calc)
        var entry3 = _builder.FuelEntry()
            .ForVehicle(vehicle.Id)
            .WithoutOdometer()
            .WithAmount(45m)
            .WithCost(67.5m)
            .WithDate(new DateOnly(2025, 12, 1))
            .Build();

        _context.FuelEntries.AddRange(entry1, entry2, entry3);
        await _context.SaveChangesAsync();

        var result = await _fuelService.GetEfficiencyAsync(vehicle.Id, _testUserId);

        result.Should().NotBeNull();

        // Only entries with odometer should be used for efficiency
        // Distance: 500 km, Fuel: 35L (only entry2 has a previous reading)
        // Efficiency: 35L / 500km * 100 = 7.0 L/100km
        result!.AverageLitersPer100Km.Should().BeApproximately(7.0m, 0.1m);

        // All entries should count for cost averages
        // Total cost: 60 + 52.5 + 67.5 = $180
        // Total amount: 40 + 35 + 45 = 120L
        result.TotalCost.Should().Be(180m);
        result.TotalAmount.Should().Be(120m); 
    }

    [Fact]
    public async Task GetEfficiencyAsync_WithNoOdometerReadings_ShouldReturnNullEfficiency()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var entry1 = _builder.FuelEntry()
            .ForVehicle(vehicle.Id)
            .WithoutOdometer()
            .WithAmount(40m)
            .WithCost(60m)
            .Build();

        var entry2 = _builder.FuelEntry()
            .ForVehicle(vehicle.Id)
            .WithoutOdometer()
            .WithAmount(35m)
            .WithCost(52.5m)
            .Build();

        _context.FuelEntries.AddRange(entry1, entry2);
        await _context.SaveChangesAsync();

        var result = await _fuelService.GetEfficiencyAsync(vehicle.Id, _testUserId);

        result.Should().NotBeNull();
        result!.AverageLitersPer100Km.Should().BeNull();
        result.AverageKilometersPerLiter.Should().BeNull();
        result.AverageCostPerKilometer.Should().BeNull();

        result.TotalCost.Should().Be(112.5m);
        result.TotalAmount.Should().Be(75m); // TotalAmount, not TotalLiters
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteFuelEntryAndLinkedExpense()
    {
        var vehicle = _builder.Vehicle().Build();
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var createDto = new CreateFuelEntryDto
        {
            EnergyType = EnergyType.Gasoline,
            Amount = 40m,
            Cost = 60m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            VehicleId = vehicle.Id
        };

        var created = await _fuelService.CreateAsync(createDto, _testUserId);
        var fuelEntry = await _context.FuelEntries
            .Include(f => f.Expense)
            .FirstAsync(f => f.Id == created!.Id);

        var linkedExpenseId = fuelEntry.ExpenseId;

        var deleteResult = await _fuelService.DeleteAsync(created!.Id, _testUserId);

        deleteResult.Should().BeTrue();

        var deletedFuelEntry = await _context.FuelEntries.FindAsync(created.Id);
        deletedFuelEntry.Should().BeNull();

        var deletedExpense = await _context.Expenses.FindAsync(linkedExpenseId);
        deletedExpense.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
