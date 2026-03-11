using DataAccess.Enums;
using Domain.Enums;

namespace DataAccess.Converters;

public static class GenderConverter
{
    public static GenderDb ToDb(Gender gender)
    {
        return gender switch
        {
            Gender.Male => GenderDb.Male,
            Gender.Female => GenderDb.Female,
            _ => throw new ArgumentOutOfRangeException(
                nameof(gender), 
                gender, 
                null)
        };
    }

    public static Gender ToDomain(GenderDb gender)
    {
        return gender switch
        {
            GenderDb.Male => Gender.Male,
            GenderDb.Female => Gender.Female,
            _ => throw new ArgumentOutOfRangeException(
                nameof(gender), 
                gender, 
                null)
        };
    }
}