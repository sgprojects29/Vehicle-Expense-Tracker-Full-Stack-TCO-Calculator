using Microsoft.AspNetCore.Identity;

namespace VehicleExpenseAPI.Models;

public class User : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
