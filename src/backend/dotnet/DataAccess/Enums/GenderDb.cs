using NpgsqlTypes;

namespace DataAccess.Enums;

[PgName("gender")] 
public enum GenderDb
{
    [PgName("Мужчина")]
    Male,
    
    [PgName("Женщина")]
    Female
}
