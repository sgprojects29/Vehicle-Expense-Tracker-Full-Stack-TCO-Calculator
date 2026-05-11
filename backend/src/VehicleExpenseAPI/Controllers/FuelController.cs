using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleExpenseAPI.DTOs.Fuel;
using VehicleExpenseAPI.Models;
using VehicleExpenseAPI.Services;

namespace VehicleExpenseAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FuelController : ControllerBase
{
    private readonly FuelService _fuelService;
    private readonly ILogger<FuelController> _logger;

    public FuelController(FuelService fuelService, ILogger<FuelController> logger)
    {
        _fuelService = fuelService;
        _logger = logger;
    }

    /// <summary>
    /// Get all fuel/charge entries for the authenticated user with optional filtering
    /// </summary>
    /// <param name="vehicleId">Optional: Filter by vehicle ID</param>
    /// <param name="energyType">Optional: Filter by energy type</param>
    /// <param name="startDate">Optional: Filter entries on or after this date (YYYY-MM-DD)</param>
    /// <param name="endDate">Optional: Filter entries on or before this date (YYYY-MM-DD)</param>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? vehicleId = null,
        [FromQuery] EnergyType? energyType = null,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetAll called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting fuel entries for user: {UserId}, VehicleId: {VehicleId}, EnergyType: {EnergyType}, StartDate: {StartDate}, EndDate: {EndDate}",
            userId, vehicleId, energyType, startDate, endDate);

        var fuelEntries = await _fuelService.GetAllAsync(userId, vehicleId, energyType, startDate, endDate);
        return Ok(fuelEntries);
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

        _logger.LogInformation("Getting fuel entry {FuelEntryId} for user: {UserId}", id, userId);
        var fuelEntry = await _fuelService.GetByIdAsync(id, userId);

        if (fuelEntry == null)
        {
            _logger.LogWarning("Fuel entry {FuelEntryId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Fuel entry not found" });
        }

        return Ok(fuelEntry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFuelEntryDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Create called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Creating new fuel entry for user: {UserId}", userId);
        var fuelEntry = await _fuelService.CreateAsync(createDto, userId);

        if (fuelEntry == null)
        {
            _logger.LogWarning("Failed to create fuel entry - vehicle {VehicleId} not found or doesn't belong to user {UserId}", 
                createDto.VehicleId, userId);
            return BadRequest(new { error = "Vehicle not found or you don't have access to it" });
        }
        
        _logger.LogInformation("Fuel entry {FuelEntryId} created successfully for user {UserId}", fuelEntry.Id, userId);
        return CreatedAtAction(nameof(GetById), new { id = fuelEntry.Id }, fuelEntry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFuelEntryDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Update called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Updating fuel entry {FuelEntryId} for user: {UserId}", id, userId);
        var (success, fuelEntry) = await _fuelService.UpdateAsync(id, updateDto, userId);

        if (!success)
        {
            _logger.LogWarning("Fuel entry {FuelEntryId} not found for user {UserId} or vehicle access denied", id, userId);
            return NotFound(new { error = "Fuel entry not found or you don't have access to the specified vehicle" });
        }

        _logger.LogInformation("Fuel entry {FuelEntryId} updated successfully for user {UserId}", id, userId);
        return Ok(fuelEntry);
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

        _logger.LogInformation("Deleting fuel entry {FuelEntryId} for user: {UserId}", id, userId);
        var success = await _fuelService.DeleteAsync(id, userId);

        if (!success)
        {
            _logger.LogWarning("Fuel entry {FuelEntryId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Fuel entry not found" });
        }

        _logger.LogInformation("Fuel entry {FuelEntryId} deleted successfully for user {UserId}", id, userId);
        return NoContent();
    }

    /// <summary>
    /// Get fuel efficiency statistics for a specific vehicle (handles hybrid with both fuel and electric)
    /// </summary>
    /// <param name="vehicleId">The vehicle ID to analyze</param>
    [HttpGet("efficiency/{vehicleId}")]
    public async Task<IActionResult> GetEfficiency(int vehicleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetEfficiency called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting fuel efficiency for vehicle {VehicleId}, user: {UserId}", vehicleId, userId);
        var efficiency = await _fuelService.GetEfficiencyAsync(vehicleId, userId);

        if (efficiency == null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for user {UserId}", vehicleId, userId);
            return NotFound(new { error = "Vehicle not found" });
        }

        return Ok(efficiency);
    }
}
