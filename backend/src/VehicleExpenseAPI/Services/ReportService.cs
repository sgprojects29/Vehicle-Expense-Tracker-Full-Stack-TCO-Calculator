using Microsoft.EntityFrameworkCore;
using System.Globalization;
using VehicleExpenseAPI.Data;
using VehicleExpenseAPI.DTOs.Report;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Services;

public class ReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Calculate Total Cost of Ownership for a specific vehicle
    /// </summary>
    public async Task<TcoReportDto?> GetTcoReportAsync(int vehicleId, string userId)
    {
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == vehicleId && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return null;
        }

        // Get all fuel entries
        var fuelEntries = await _context.FuelEntries
            .Where(f => f.VehicleId == vehicleId)
            .ToListAsync();

        // Get all expenses
        var expenses = await _context.Expenses
            .Where(e => e.VehicleId == vehicleId)
            .ToListAsync();

        // Calculate costs
        var totalFuelCost = fuelEntries.Sum(f => f.Cost);
        var totalExpensesCost = expenses.Sum(e => e.Amount);
        var totalCost = vehicle.PurchasePrice + totalFuelCost + totalExpensesCost;

        // Calculate ownership period
        var endDate = vehicle.OwnershipEnd ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var ownershipDays = endDate.DayNumber - vehicle.OwnershipStart.DayNumber;
        var ownershipMonths = ownershipDays / 30.0m;

        // Group expenses by category
        var expensesByCategory = expenses
            .GroupBy(e => e.Category)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Sum(e => e.Amount)
            );

        // Calculate per-kilometer metrics (if we have fuel data)
        decimal? totalKm = null;
        decimal? costPerKm = null;
        decimal? fuelCostPerKm = null;
        decimal? expensesCostPerKm = null;

        if (fuelEntries.Count >= 2)
        {
            var orderedEntries = fuelEntries.OrderBy(f => f.Odometer).ToList();
            totalKm = orderedEntries.Last().Odometer - orderedEntries.First().Odometer;

            if (totalKm > 0)
            {
                costPerKm = totalCost / totalKm.Value;
                fuelCostPerKm = totalFuelCost / totalKm.Value;
                expensesCostPerKm = totalExpensesCost / totalKm.Value;
            }
        }

        return new TcoReportDto
        {
            VehicleId = vehicle.Id,
            VehicleMake = vehicle.Make,
            VehicleModel = vehicle.Model,
            VehicleYear = vehicle.Year,
            PurchasePrice = vehicle.PurchasePrice,
            OwnershipStart = vehicle.OwnershipStart,
            OwnershipEnd = vehicle.OwnershipEnd,
            OwnershipDays = ownershipDays,
            TotalFuelCost = totalFuelCost,
            TotalExpensesCost = totalExpensesCost,
            TotalCost = totalCost,
            ExpensesByCategory = expensesByCategory,
            TotalKilometers = totalKm,
            CostPerKilometer = costPerKm.HasValue ? Math.Round(costPerKm.Value, 4) : null,
            FuelCostPerKilometer = fuelCostPerKm.HasValue ? Math.Round(fuelCostPerKm.Value, 4) : null,
            ExpensesCostPerKilometer = expensesCostPerKm.HasValue ? Math.Round(expensesCostPerKm.Value, 4) : null,
            CostPerDay = ownershipDays > 0 ? Math.Round(totalCost / ownershipDays, 2) : 0,
            CostPerMonth = ownershipMonths > 0 ? Math.Round(totalCost / ownershipMonths, 2) : 0,
            TotalFuelEntries = fuelEntries.Count,
            TotalExpenseEntries = expenses.Count
        };
    }

    /// <summary>
    /// Get cost breakdown by category for a vehicle
    /// </summary>
    public async Task<CostBreakdownDto?> GetCostBreakdownAsync(int vehicleId, string userId)
    {
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == vehicleId && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return null;
        }

        var fuelEntries = await _context.FuelEntries
            .Where(f => f.VehicleId == vehicleId)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.VehicleId == vehicleId)
            .ToListAsync();

        // FuelEntries that have linked Expenses should not be counted in expense categories
        var fuelEntryExpenseIds = fuelEntries
            .Where(f => f.ExpenseId.HasValue)
            .Select(f => f.ExpenseId!.Value)
            .ToHashSet();

        // Total fuel cost = all FuelEntry costs
        var totalFuelCost = fuelEntries.Sum(f => f.Cost);
        
        // Total expenses (excluding expenses that are linked to FuelEntries)
        var totalExpensesCost = expenses
            .Where(e => !fuelEntryExpenseIds.Contains(e.Id))
            .Sum(e => e.Amount);
        
        var totalCost = vehicle.PurchasePrice + totalFuelCost + totalExpensesCost;

        // Build category breakdown
        var categoryBreakdown = new List<CategoryBreakdownItem>();

        // Add purchase price as a category
        if (vehicle.PurchasePrice > 0)
        {
            categoryBreakdown.Add(new CategoryBreakdownItem
            {
                Category = "Purchase Price",
                Amount = vehicle.PurchasePrice,
                Percentage = totalCost > 0 ? Math.Round((vehicle.PurchasePrice / totalCost) * 100, 2) : 0,
                Count = 1
            });
        }

        // Add combined fuel costs as a single category
        if (totalFuelCost > 0)
        {
            categoryBreakdown.Add(new CategoryBreakdownItem
            {
                Category = "Fuel & Charging",
                Amount = totalFuelCost,
                Percentage = totalCost > 0 ? Math.Round((totalFuelCost / totalCost) * 100, 2) : 0,
                Count = fuelEntries.Count
            });
        }

        // Add expense categories (EXCLUDE expenses linked to FuelEntries)
        var expenseGroups = expenses
            .Where(e => !fuelEntryExpenseIds.Contains(e.Id))
            .GroupBy(e => e.Category)
            .Select(g => new CategoryBreakdownItem
            {
                Category = g.Key.ToString(),
                Amount = g.Sum(e => e.Amount),
                Percentage = totalCost > 0 ? Math.Round((g.Sum(e => e.Amount) / totalCost) * 100, 2) : 0,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Amount);

        categoryBreakdown.AddRange(expenseGroups);

        return new CostBreakdownDto
        {
            VehicleId = vehicle.Id,
            VehicleMake = vehicle.Make,
            VehicleModel = vehicle.Model,
            PurchasePrice = vehicle.PurchasePrice,
            TotalFuelCost = totalFuelCost,
            TotalExpensesCost = totalExpensesCost,
            TotalCost = totalCost,
            CategoryBreakdown = categoryBreakdown
        };
    }

    /// <summary>
    /// Get monthly cost trends for a vehicle
    /// </summary>
    public async Task<MonthlyCostTrendDto?> GetMonthlyCostTrendAsync(int vehicleId, string userId)
    {
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == vehicleId && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return null;
        }

        var fuelEntries = await _context.FuelEntries
            .Where(f => f.VehicleId == vehicleId)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.VehicleId == vehicleId)
            .ToListAsync();

        // Group by year and month
        var fuelByMonth = fuelEntries
            .GroupBy(f => new { f.Date.Year, f.Date.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => (Cost: g.Sum(f => f.Cost), Count: g.Count())
            );

        var expensesByMonth = expenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => (Cost: g.Sum(e => e.Amount), Count: g.Count())
            );

        // Get all unique months
        var allMonths = fuelByMonth.Keys
            .Union(expensesByMonth.Keys)
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        var monthlyData = allMonths.Select(m =>
        {
            var fuelCost = fuelByMonth.TryGetValue(m, out var fData) ? fData.Cost : 0;
            var fuelCount = fuelByMonth.TryGetValue(m, out var fData2) ? fData2.Count : 0;
            var expensesCost = expensesByMonth.TryGetValue(m, out var eData) ? eData.Cost : 0;
            var expensesCount = expensesByMonth.TryGetValue(m, out var eData2) ? eData2.Count : 0;

            return new MonthlyDataPoint
            {
                Year = m.Year,
                Month = m.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month),
                FuelCost = fuelCost,
                ExpensesCost = expensesCost,
                TotalCost = fuelCost + expensesCost,
                FuelEntries = fuelCount,
                ExpenseEntries = expensesCount
            };
        }).ToList();

        return new MonthlyCostTrendDto
        {
            VehicleId = vehicle.Id,
            VehicleMake = vehicle.Make,
            VehicleModel = vehicle.Model,
            MonthlyData = monthlyData
        };
    }

    /// <summary>
    /// Get summary of all vehicles for the user
    /// </summary>
    public async Task<VehicleSummaryDto> GetVehicleSummaryAsync(string userId)
    {
        var vehicles = await _context.Vehicles
            .Where(v => v.UserId == userId)
            .ToListAsync();

        var summary = new VehicleSummaryDto
        {
            TotalVehicles = vehicles.Count,
            TotalInvestment = vehicles.Sum(v => v.PurchasePrice)
        };

        foreach (var vehicle in vehicles)
        {
            var fuelCost = await _context.FuelEntries
                .Where(f => f.VehicleId == vehicle.Id)
                .SumAsync(f => f.Cost);

            var expensesCost = await _context.Expenses
                .Where(e => e.VehicleId == vehicle.Id)
                .SumAsync(e => e.Amount);

            var totalCost = vehicle.PurchasePrice + fuelCost + expensesCost;

            // Calculate monthly average
            var endDate = vehicle.OwnershipEnd ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var ownershipDays = endDate.DayNumber - vehicle.OwnershipStart.DayNumber;
            var ownershipMonths = ownershipDays / 30.0m;
            var monthlyAverage = ownershipMonths > 0 ? totalCost / ownershipMonths : 0;

            summary.Vehicles.Add(new VehicleSummaryItem
            {
                VehicleId = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                PurchasePrice = vehicle.PurchasePrice,
                TotalCost = totalCost,
                MonthlyAverage = Math.Round(monthlyAverage, 2)
            });

            summary.TotalFuelCost += fuelCost;
            summary.TotalExpensesCost += expensesCost;
        }

        summary.GrandTotalCost = summary.TotalInvestment + summary.TotalFuelCost + summary.TotalExpensesCost;

        return summary;
    }
}
