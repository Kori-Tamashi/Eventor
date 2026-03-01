using NpgsqlTypes;

namespace Domain.Enums;

public enum Gender
{
    [PgName("Мужчина")]
    Male,
    
    [PgName("Женщина")]
    Female
}