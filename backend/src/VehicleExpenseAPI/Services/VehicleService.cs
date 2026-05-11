using Microsoft.EntityFrameworkCore;
using VehicleExpenseAPI.Data;
using VehicleExpenseAPI.DTOs.Vehicle;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Services;

public class VehicleService
{
    private readonly ApplicationDbContext _context;

    public VehicleService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static string GetVehicleTypeDisplay(VehicleType vehicleType)
{
        return vehicleType switch
        {
            VehicleType.Gasoline => "Gasoline",
            VehicleType.Diesel => "Diesel",
            VehicleType.Electric => "Electric",
            VehicleType.Hybrid => "Hybrid",
            VehicleType.PlugInHybrid => "Plug-in Hybrid",
            _ => "Unknown"
        };
    }

    public async Task<IEnumerable<VehicleDto>> GetAllAsync(string userId)
    {
        var vehicles = await _context.Vehicles
            .Where(v => v.UserId == userId)
            .ToListAsync();

        return vehicles.Select(v => new VehicleDto
        {
            Id = v.Id,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            PurchasePrice = v.PurchasePrice,
            OwnershipStart = v.OwnershipStart,
            OwnershipEnd = v.OwnershipEnd,
            UserId = v.UserId,
            VehicleType = v.VehicleType,
            VehicleTypeDisplay = GetVehicleTypeDisplay(v.VehicleType)
        });
    }

    public async Task<VehicleDto?> GetByIdAsync(int id, string userId)
    {
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == id && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return null;
        }

        return new VehicleDto
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            PurchasePrice = vehicle.PurchasePrice,
            OwnershipStart = vehicle.OwnershipStart,
            OwnershipEnd = vehicle.OwnershipEnd,
            UserId = vehicle.UserId,
            VehicleType = vehicle.VehicleType,
            VehicleTypeDisplay = GetVehicleTypeDisplay(vehicle.VehicleType)
        };
    }

    public async Task<VehicleDto> CreateAsync(CreateVehicleDto createDto, string userId)
    {
        var vehicle = new Vehicle
        {
            Make = createDto.Make,
            Model = createDto.Model,
            Year = createDto.Year,
            PurchasePrice = createDto.PurchasePrice,
            OwnershipStart = createDto.OwnershipStart,
            OwnershipEnd = createDto.OwnershipEnd,
            UserId = userId,
            VehicleType = createDto.VehicleType
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return new VehicleDto
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            PurchasePrice = vehicle.PurchasePrice,
            OwnershipStart = vehicle.OwnershipStart,
            OwnershipEnd = vehicle.OwnershipEnd,
            UserId = vehicle.UserId,
            VehicleType = vehicle.VehicleType,
            VehicleTypeDisplay = GetVehicleTypeDisplay(vehicle.VehicleType)
        };
    }

    public async Task<(bool Success, VehicleDto? Vehicle)> UpdateAsync(int id, UpdateVehicleDto updateDto, string userId)
    {
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == id && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return (false, null);
        }

        vehicle.Make = updateDto.Make;
        vehicle.Model = updateDto.Model;
        vehicle.Year = updateDto.Year;
        vehicle.PurchasePrice = updateDto.PurchasePrice;
        vehicle.OwnershipStart = updateDto.OwnershipStart;
        vehicle.OwnershipEnd = updateDto.OwnershipEnd;
        vehicle.VehicleType = updateDto.VehicleType;

        await _context.SaveChangesAsync();

        return (true, new VehicleDto
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            PurchasePrice = vehicle.PurchasePrice,
            OwnershipStart = vehicle.OwnershipStart,
            OwnershipEnd = vehicle.OwnershipEnd,
            UserId = vehicle.UserId,
            VehicleType = vehicle.VehicleType,
            VehicleTypeDisplay = GetVehicleTypeDisplay(vehicle.VehicleType)
        });
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var vehicle = await _context.Vehicles
            .Where(v => v.Id == id && v.UserId == userId)
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return false;
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        return true;
    }
}
