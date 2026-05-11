using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleExpenseAPI.DTOs.Expense;
using VehicleExpenseAPI.Services;

namespace VehicleExpenseAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseService _expenseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(ExpenseService expenseService, ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all expenses for the authenticated user with optional filtering
    /// </summary>
    /// <param name="vehicleId">Optional: Filter by vehicle ID</param>
    /// <param name="category">Optional: Filter by category (0=Fuel, 1=Maintenance, etc.)</param>
    /// <param name="startDate">Optional: Filter expenses on or after this date (YYYY-MM-DD)</param>
    /// <param name="endDate">Optional: Filter expenses on or before this date (YYYY-MM-DD)</param>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? vehicleId = null, 
        [FromQuery] int? category = null,   
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetAll called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Getting expenses for user: {UserId}, VehicleId: {VehicleId}, Category: {Category}, StartDate: {StartDate}, EndDate: {EndDate}", 
            userId, vehicleId, category, startDate, endDate);
        
        var expenses = await _expenseService.GetAllAsync(userId, vehicleId, category, startDate, endDate);
        return Ok(expenses);
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

        _logger.LogInformation("Getting expense {ExpenseId} for user: {UserId}", id, userId);
        var expense = await _expenseService.GetByIdAsync(id, userId);

        if (expense == null)
        {
            _logger.LogWarning("Expense {ExpenseId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Expense not found" });
        }

        return Ok(expense);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Create called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Creating new expense for user: {UserId}", userId);
        var expense = await _expenseService.CreateAsync(createDto, userId);

        if (expense == null)
        {
            _logger.LogWarning("Failed to create expense - vehicle {VehicleId} not found or doesn't belong to user {UserId}", 
                createDto.VehicleId, userId);
            return BadRequest(new { error = "Vehicle not found or you don't have access to it" });
        }
        
        _logger.LogInformation("Expense {ExpenseId} created successfully for user {UserId}", expense.Id, userId);
        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Update called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        _logger.LogInformation("Updating expense {ExpenseId} for user: {UserId}", id, userId);
        var (success, expense) = await _expenseService.UpdateAsync(id, updateDto, userId);

        if (!success)
        {
            _logger.LogWarning("Expense {ExpenseId} not found for user {UserId} or vehicle access denied", id, userId);
            return NotFound(new { error = "Expense not found or you don't have access to the specified vehicle" });
        }

        _logger.LogInformation("Expense {ExpenseId} updated successfully for user {UserId}", id, userId);
        return Ok(expense);
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

        _logger.LogInformation("Deleting expense {ExpenseId} for user: {UserId}", id, userId);
        var success = await _expenseService.DeleteAsync(id, userId);

        if (!success)
        {
            _logger.LogWarning("Expense {ExpenseId} not found for user {UserId}", id, userId);
            return NotFound(new { error = "Expense not found" });
        }

        _logger.LogInformation("Expense {ExpenseId} deleted successfully for user {UserId}", id, userId);
        return NoContent();
    }
}
