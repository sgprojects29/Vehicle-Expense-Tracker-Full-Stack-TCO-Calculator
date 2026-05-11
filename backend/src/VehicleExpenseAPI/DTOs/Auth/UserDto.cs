namespace VehicleExpenseAPI.DTOs.Auth;

public class UserDto
{
    public string Id { get; set; } = string.Empty; // GUID
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
