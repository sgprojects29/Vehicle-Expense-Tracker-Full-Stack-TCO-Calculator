using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleExpenseAPI.DTOs.Vehicle;
using VehicleExpenseAPI.Services;

namespace VehicleExpenseAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _vehicleService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(VehicleService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetAll called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting all vehicles for user: {UserId}", userId);
        var vehicles = await _vehicleService.GetAllAsync(userId);
        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetById called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting vehicle {VehicleId} for user: {UserId}", id, userId);
        var vehicle = await _vehicleService.GetByIdAsync(id, userId);

        if (vehicle == null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Vehicle not found" });
        }

        return Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Create called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Creating new vehicle for user: {UserId}", userId);
        var vehicle = await _vehicleService.CreateAsync(createDto, userId);
        
        _logger.LogInformation("Vehicle {VehicleId} created successfully for user {UserId}", vehicle.Id, userId);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Update called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Updating vehicle {VehicleId} for user: {UserId}", id, userId);
        var (success, vehicle) = await _vehicleService.UpdateAsync(id, updateDto, userId);

        if (!success)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Vehicle not found" });
        }

        _logger.LogInformation("Vehicle {VehicleId} updated successfully for user {UserId}", id, userId);
        return Ok(vehicle);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Delete called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Deleting vehicle {VehicleId} for user: {UserId}", id, userId);
        var success = await _vehicleService.DeleteAsync(id, userId);

        if (!success)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Vehicle not found" });
        }

        _logger.LogInformation("Vehicle {VehicleId} deleted successfully for user {UserId}", id, userId);
        return NoContent();
    }
}
