namespace Web.Dtos;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
    public string? PasswordHash { get; set; }
}