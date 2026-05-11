using Microsoft.EntityFrameworkCore;
using VehicleExpenseAPI.Data;
using VehicleExpenseAPI.DTOs.Fuel;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Services;

public class FuelService
{
    private readonly ApplicationDbContext _context;

    public FuelService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FuelEntryDto>> GetAllAsync(
        string userId,
        int? vehicleId = null,
        EnergyType? energyType = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var query = _context.FuelEntries
            .Include(f => f.Vehicle)
            .Where(f => f.Vehicle!.UserId == userId);

        if (vehicleId.HasValue)
        {
            query = query.Where(f => f.VehicleId == vehicleId.Value);
        }

        if (energyType.HasValue)
        {
            query = query.Where(f => f.EnergyType == energyType.Value);
        }

        // Filter by start date if specified
        if (startDate.HasValue)
        {
            query = query.Where(f => f.Date >= startDate.Value);
        }

        // Filter by end date if specified
        if (endDate.HasValue)
        {
            query = query.Where(f => f.Date <= endDate.Value);
        }

        var fuelEntries = await query
            .OrderByDescending(f => f.Date)
            .ThenByDescending(f => f.Odometer)
            .ToListAsync();

        return fuelEntries.Select(f => MapToDto(f));
    }

    public async Task<FuelEntryDto?> GetByIdAsync(int id, string userId)
    {
        var fuelEntry = await _context.FuelEntries
            .Include(f => f.Vehicle)
            .Where(f => f.Id == id && f.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (fuelEntry == null)
        {
            return null;
        }

        return MapToDto(fuelEntry);
    }

    public async Task<FuelEntryDto?> CreateAsync(CreateFuelEntryDto createDto, string userId)
    {
        // Verify the vehicle belongs to the user
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == createDto.VehicleId && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return null;
        }

        // Create the Fuel Expense first (so it gets an ID)
        var fuelNote = createDto.EnergyType == EnergyType.Electricity
            ? $"Charging session: {createDto.Amount} kWh"
            : $"{(createDto.EnergyType == EnergyType.Diesel ? "Diesel" : "Gasoline")} fill-up: {createDto.Amount}L";

        var fuelExpense = new Expense
        {
            Category = ExpenseCategory.Other,
            Amount = createDto.Cost,
            Date = createDto.Date,
            Notes = fuelNote,
            VehicleId = createDto.VehicleId
        };

        _context.Expenses.Add(fuelExpense);
        await _context.SaveChangesAsync(); // Save to get the Expense ID

        // Now create the FuelEntry linked to the Expense
        var fuelEntry = new FuelEntry
        {
            EnergyType = createDto.EnergyType,
            Amount = createDto.Amount,
            Cost = createDto.Cost,
            Odometer = createDto.Odometer,
            Date = createDto.Date,
            VehicleId = createDto.VehicleId,
            Vehicle = vehicle,
            ExpenseId = fuelExpense.Id
        };

        _context.FuelEntries.Add(fuelEntry);
        await _context.SaveChangesAsync();

        return MapToDto(fuelEntry);
    }

    public async Task<(bool Success, FuelEntryDto? FuelEntry)> UpdateAsync(int id, UpdateFuelEntryDto updateDto, string userId)
    {
        var fuelEntry = await _context.FuelEntries
            .Include(f => f.Vehicle)
            .Include(f => f.Expense) // Include the linked expense
            .Where(f => f.Id == id && f.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (fuelEntry == null)
        {
            return (false, null);
        }

        // Update FuelEntry fields (only if provided)
        if (updateDto.EnergyType.HasValue)
            fuelEntry.EnergyType = updateDto.EnergyType.Value;
        
        if (updateDto.Amount.HasValue)
            fuelEntry.Amount = updateDto.Amount.Value;
        
        if (updateDto.Cost.HasValue)
            fuelEntry.Cost = updateDto.Cost.Value;
        
        if (updateDto.Odometer.HasValue)
            fuelEntry.Odometer = updateDto.Odometer.Value;
        
        if (updateDto.Date.HasValue)
            fuelEntry.Date = updateDto.Date.Value;

        // Update the linked Expense to match
        if (fuelEntry.Expense != null)
        {
            if (updateDto.Cost.HasValue)
                fuelEntry.Expense.Amount = updateDto.Cost.Value;
            
            if (updateDto.Date.HasValue)
                fuelEntry.Expense.Date = updateDto.Date.Value;
            
            // Update notes if energy type or amount changed
            if (updateDto.EnergyType.HasValue || updateDto.Amount.HasValue)
            {
                var energyType = updateDto.EnergyType ?? fuelEntry.EnergyType;
                var amount = updateDto.Amount ?? fuelEntry.Amount;
                
                fuelEntry.Expense.Notes = energyType == EnergyType.Electricity
                    ? $"Charging session: {amount} kWh"
                    : $"{(energyType == EnergyType.Diesel ? "Diesel" : "Gasoline")} fill-up: {amount}L";
            }
        }

        await _context.SaveChangesAsync();

        return (true, MapToDto(fuelEntry));
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var fuelEntry = await _context.FuelEntries
            .Include(f => f.Vehicle)
            .Include(f => f.Expense) // Include the linked expense
            .Where(f => f.Id == id && f.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (fuelEntry == null)
        {
            return false;
        }

        // Delete the linked Fuel Expense too
        if (fuelEntry.Expense != null)
        {
            _context.Expenses.Remove(fuelEntry.Expense);
        }

        _context.FuelEntries.Remove(fuelEntry);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Calculate fuel efficiency statistics for a specific vehicle (handles hybrid vehicles)
    /// </summary>
    public async Task<FuelEfficiencyDto?> GetEfficiencyAsync(int vehicleId, string userId)
    {
        // Verify vehicle ownership
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == vehicleId && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return null;
        }

        // Get all fuel entries for the vehicle
        var allEntries = await _context.FuelEntries
            .Where(f => f.VehicleId == vehicleId)
            .ToListAsync();

        // Calculate total costs (all entries)
        var totalCost = allEntries.Sum(f => f.Cost);
        var totalAmount = allEntries.Sum(f => f.Amount);

        // Only use entries with valid odometer readings for efficiency calculations
        var entriesWithOdometer = allEntries
            .Where(e => e.Odometer.HasValue && e.Odometer.Value > 0)
            .OrderBy(e => e.Odometer)
            .ToList();

        // If we don't have enough odometer data, return costs only
        if (entriesWithOdometer.Count < 2)
        {
            return new FuelEfficiencyDto
            {
                VehicleId = vehicleId,
                TotalCost = totalCost,
                TotalAmount = totalAmount,
                TotalKilometers = 0,
                AverageLitersPer100Km = null,
                AverageKilometersPerLiter = null,
                AverageCostPerKilometer = null,
                AverageCostPerFillUp = allEntries.Count > 0 ? totalCost / allEntries.Count : 0,
                TotalFillUps = allEntries.Count,
                EntriesWithOdometer = entriesWithOdometer.Count,
                EntriesWithoutOdometer = allEntries.Count - entriesWithOdometer.Count
            };
        }

        // Calculate efficiency metrics using entries with odometer
        var firstEntry = entriesWithOdometer.First();
        var lastEntry = entriesWithOdometer.Last();
        var totalKm = lastEntry.Odometer!.Value - firstEntry.Odometer!.Value;

        // Only calculate efficiency if we've traveled some distance
        if (totalKm <= 0)
        {
            return new FuelEfficiencyDto
            {
                VehicleId = vehicleId,
                TotalCost = totalCost,
                TotalAmount = totalAmount,
                TotalKilometers = 0,
                AverageLitersPer100Km = null,
                AverageKilometersPerLiter = null,
                AverageCostPerKilometer = null,
                AverageCostPerFillUp = allEntries.Count > 0 ? totalCost / allEntries.Count : 0,
                TotalFillUps = allEntries.Count,
                EntriesWithOdometer = entriesWithOdometer.Count,
                EntriesWithoutOdometer = allEntries.Count - entriesWithOdometer.Count
            };
        }

        // Calculate efficiency using only entries with odometer (excluding first entry)
        var fuelUsedForEfficiency = entriesWithOdometer.Skip(1).Sum(f => f.Amount);
        var averageLitersPer100Km = (fuelUsedForEfficiency / totalKm) * 100;
        var averageKmPerLiter = totalKm / fuelUsedForEfficiency;
        var averageCostPerKm = totalCost / totalKm;

        return new FuelEfficiencyDto
        {
            VehicleId = vehicleId,
            TotalCost = totalCost,
            TotalAmount = totalAmount,
            TotalKilometers = totalKm,
            AverageLitersPer100Km = Math.Round(averageLitersPer100Km, 2),
            AverageKilometersPerLiter = Math.Round(averageKmPerLiter, 2),
            AverageCostPerKilometer = Math.Round(averageCostPerKm, 4),
            AverageCostPerFillUp = allEntries.Count > 0 ? Math.Round(totalCost / allEntries.Count, 2) : 0,
            TotalFillUps = allEntries.Count,
            EntriesWithOdometer = entriesWithOdometer.Count,
            EntriesWithoutOdometer = allEntries.Count - entriesWithOdometer.Count
        };
    }

    private static FuelEntryDto MapToDto(FuelEntry fuelEntry)
    {
        var unit = fuelEntry.EnergyType == EnergyType.Electricity ? "kWh" : "L";
        var energyTypeDisplay = fuelEntry.EnergyType switch
        {
            EnergyType.Gasoline => "Gasoline",
            EnergyType.Diesel => "Diesel",
            EnergyType.Electricity => "Electricity",
            _ => "Unknown"
        };

        return new FuelEntryDto
        {
            Id = fuelEntry.Id,
            EnergyType = fuelEntry.EnergyType,
            EnergyTypeDisplay = energyTypeDisplay,
            Amount = fuelEntry.Amount,
            Unit = unit,
            Cost = fuelEntry.Cost,
            Odometer = fuelEntry.Odometer,
            Date = fuelEntry.Date,
            VehicleId = fuelEntry.VehicleId,
            VehicleMake = fuelEntry.Vehicle!.Make,
            VehicleModel = fuelEntry.Vehicle!.Model,
            VehicleType = fuelEntry.Vehicle!.VehicleType,
            CostPerUnit = fuelEntry.Amount > 0 ? Math.Round(fuelEntry.Cost / fuelEntry.Amount, 2) : 0
        };
    }
}
