using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleExpenseAPI.Services;

namespace VehicleExpenseAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Get Total Cost of Ownership (TCO) report for a specific vehicle
    /// </summary>
    /// <param name="vehicleId">The vehicle ID</param>
    [HttpGet("tco/{vehicleId}")]
    public async Task<IActionResult> GetTcoReport(int vehicleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetTcoReport called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting TCO report for vehicle {VehicleId}, user: {UserId}", vehicleId, userId);
        var report = await _reportService.GetTcoReportAsync(vehicleId, userId);

        if (report == null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for user {UserId}", vehicleId, userId);
            return NotFound(new { error = "Vehicle not found" });
        }

        return Ok(report);
    }

    /// <summary>
    /// Get cost breakdown by category for a specific vehicle
    /// </summary>
    /// <param name="vehicleId">The vehicle ID</param>
    [HttpGet("breakdown/{vehicleId}")]
    public async Task<IActionResult> GetCostBreakdown(int vehicleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetCostBreakdown called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting cost breakdown for vehicle {VehicleId}, user: {UserId}", vehicleId, userId);
        var breakdown = await _reportService.GetCostBreakdownAsync(vehicleId, userId);

        if (breakdown == null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for user {UserId}", vehicleId, userId);
            return NotFound(new { error = "Vehicle not found" });
        }

        return Ok(breakdown);
    }

    /// <summary>
    /// Get monthly cost trends for a specific vehicle
    /// </summary>
    /// <param name="vehicleId">The vehicle ID</param>
    [HttpGet("trends/{vehicleId}")]
    public async Task<IActionResult> GetMonthlyCostTrend(int vehicleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetMonthlyCostTrend called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting monthly cost trends for vehicle {VehicleId}, user: {UserId}", vehicleId, userId);
        var trends = await _reportService.GetMonthlyCostTrendAsync(vehicleId, userId);

        if (trends == null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found for user {UserId}", vehicleId, userId);
            return NotFound(new { error = "Vehicle not found" });
        }

        return Ok(trends);
    }

    /// <summary>
    /// Get summary of all vehicles for the authenticated user (dashboard overview)
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetVehicleSummary()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetVehicleSummary called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting vehicle summary for user: {UserId}", userId);
        var summary = await _reportService.GetVehicleSummaryAsync(userId);

        return Ok(summary);
    }
}
