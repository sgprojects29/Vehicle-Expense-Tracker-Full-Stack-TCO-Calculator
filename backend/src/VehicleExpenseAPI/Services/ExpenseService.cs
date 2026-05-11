using Microsoft.EntityFrameworkCore;
using VehicleExpenseAPI.Data;
using VehicleExpenseAPI.DTOs.Expense;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Services;

public class ExpenseService
{
    private readonly ApplicationDbContext _context;

    public ExpenseService(ApplicationDbContext context)
    {
        _context = context;
    }

public async Task<IEnumerable<ExpenseDto>> GetAllAsync(
        string userId, 
        int? vehicleId = null, 
        int? category = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var query = _context.Expenses
            .Include(e => e.Vehicle)
            .Where(e => e.Vehicle!.UserId == userId);  // Filter by user's vehicles

        // Filter by vehicle if specified
        if (vehicleId.HasValue)
        {
            query = query.Where(e => e.VehicleId == vehicleId.Value);
        }

        // Filter by category if specified
        if (category.HasValue)
        {
            query = query.Where(e => (int)e.Category == category.Value);
        }

        // Filter by start date if specified
        if (startDate.HasValue)
        {
            query = query.Where(e => e.Date >= startDate.Value);
        }

        // Filter by end date if specified
        if (endDate.HasValue)
        {
            query = query.Where(e => e.Date <= endDate.Value);
        }

        var expenses = await query
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        return expenses.Select(e => new ExpenseDto
        {
            Id = e.Id,
            Category = (int)e.Category,
            CategoryName = e.Category.ToString(),
            Amount = e.Amount,
            Date = e.Date,
            Notes = e.Notes,
            VehicleId = e.VehicleId,
            VehicleMake = e.Vehicle?.Make,
            VehicleModel = e.Vehicle?.Model
        });
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id, string userId)
    {
        var expense = await _context.Expenses
            .Include(e => e.Vehicle)
            .Where(e => e.Id == id && e.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (expense == null)
        {
            return null;
        }

        return new ExpenseDto
        {
            Id = expense.Id,
            Category = (int)expense.Category,
            CategoryName = expense.Category.ToString(),
            Amount = expense.Amount,
            Date = expense.Date,
            Notes = expense.Notes,
            VehicleId = expense.VehicleId,
            VehicleMake = expense.Vehicle?.Make,
            VehicleModel = expense.Vehicle?.Model
        };
    }

    public async Task<ExpenseDto?> CreateAsync(CreateExpenseDto createDto, string userId)
    {
        // Verify the vehicle belongs to the user
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == createDto.VehicleId && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return null;  // Vehicle not found or doesn't belong to user
        }

        var expense = new Expense
        {
            Category = (ExpenseCategory)createDto.Category,
            Amount = createDto.Amount,
            Date = createDto.Date,
            Notes = createDto.Notes,
            VehicleId = createDto.VehicleId
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return new ExpenseDto
        {
            Id = expense.Id,
            Category = (int)expense.Category,
            CategoryName = expense.Category.ToString(),
            Amount = expense.Amount,
            Date = expense.Date,
            Notes = expense.Notes,
            VehicleId = expense.VehicleId,
            VehicleMake = vehicle.Make,
            VehicleModel = vehicle.Model
        };
    }

    public async Task<(bool Success, ExpenseDto? Expense)> UpdateAsync(int id, UpdateExpenseDto updateDto, string userId)
    {
        var expense = await _context.Expenses
            .Include(e => e.Vehicle)
            .Where(e => e.Id == id && e.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (expense == null)
        {
            return (false, null);
        }

        // If changing vehicles, verify the new vehicle belongs to the user
        if (expense.VehicleId != updateDto.VehicleId)
        {
            var newVehicle = await _context.Vehicles
                .Where(v => v.Id == updateDto.VehicleId && v.UserId == userId)
                .FirstOrDefaultAsync();

            if (newVehicle == null)
            {
                return (false, null);  // New vehicle not found or doesn't belong to user
            }
        }

        expense.Category = (ExpenseCategory)updateDto.Category;
        expense.Amount = updateDto.Amount;
        expense.Date = updateDto.Date;
        expense.Notes = updateDto.Notes;
        expense.VehicleId = updateDto.VehicleId;

        await _context.SaveChangesAsync();

        // Reload to get updated vehicle info if changed
        await _context.Entry(expense).Reference(e => e.Vehicle).LoadAsync();

        return (true, new ExpenseDto
        {
            Id = expense.Id,
            Category = (int)expense.Category,
            CategoryName = expense.Category.ToString(),
            Amount = expense.Amount,
            Date = expense.Date,
            Notes = expense.Notes,
            VehicleId = expense.VehicleId,
            VehicleMake = expense.Vehicle?.Make,
            VehicleModel = expense.Vehicle?.Model
        });
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var expense = await _context.Expenses
            .Include(e => e.Vehicle)
            .Where(e => e.Id == id && e.Vehicle!.UserId == userId)
            .FirstOrDefaultAsync();

        if (expense == null)
        {
            return false;
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return true;
    }
}
