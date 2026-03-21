using DataAccess.Enums;
using Domain.Enums;

namespace DataAccess.Converters;

public static class GenderConverter
{
    public static GenderDb ToDb(Gender gender) => gender switch
    {
        Gender.Male => GenderDb.Male,
        Gender.Female => GenderDb.Female,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gender),
            gender,
            $"Unknown gender value: {gender}")
    };

    public static Gender ToDomain(GenderDb gender) => gender switch
    {
        GenderDb.Male => Gender.Male,
        GenderDb.Female => Gender.Female,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gender),
            gender,
            $"Unknown gender value: {gender}")
    };
}