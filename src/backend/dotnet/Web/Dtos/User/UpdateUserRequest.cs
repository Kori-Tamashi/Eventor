namespace Web.Dtos;

public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public Gender? Gender { get; set; }
    public UserRole? Role { get; set; }
    public string? Password { get; set; }
}