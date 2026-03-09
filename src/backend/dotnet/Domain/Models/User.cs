using Domain.Enums;

namespace Domain.Models;

public class User
{
    public User() { }
    
    public User(Guid id, 
        string name, 
        string phone, 
        Gender gender, 
        UserRole role, 
        string passwordHash)
    {
        Id = id;
        Name = name;
        Phone = phone;
        Gender = gender;
        Role = role;
        PasswordHash = passwordHash;
    }
    
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
    public string PasswordHash { get; set; }
}

