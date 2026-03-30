using Domain.Enums;
using Domain.Models;

namespace Tests.Core.Fixtures;

public class UserFixture
{
    private Guid? _id;
    private string _name = "Test User";
    private string _phone = "+1234567890";
    private Gender _gender = Gender.Male;
    private UserRole _role = UserRole.User;
    private string _passwordHash = "test_hash";

    public static UserFixture Default() => new();
    
    public UserFixture WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserFixture WithName(string name)
    {
        _name = name;
        return this;
    }

    public UserFixture WithPhone(string phone)
    {
        _phone = phone;
        return this;
    }

    public UserFixture WithGender(Gender gender)
    {
        _gender = gender;
        return this;
    }

    public UserFixture WithRole(UserRole role)
    {
        _role = role;
        return this;
    }

    public UserFixture WithPasswordHash(string hash)
    {
        _passwordHash = hash;
        return this;
    }

    public User Build()
    {
        return new User(
            id: _id ?? Guid.NewGuid(),
            name: _name,
            phone: _phone,
            gender: _gender,
            role: _role,
            passwordHash: _passwordHash
        );
    }
}