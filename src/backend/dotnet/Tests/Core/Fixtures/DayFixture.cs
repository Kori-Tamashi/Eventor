using Domain.Models;

namespace Tests.Core.Fixtures;

public class DayFixture
{
    private Guid? _id;
    private Guid? _eventId;
    private Guid? _menuId;
    private string _title = "Default Day";
    private string _description = "Default Description";
    private int _sequenceNumber = 1;

    public static DayFixture Default() => new();

    public DayFixture WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public DayFixture WithEventId(Guid eventId)
    {
        _eventId = eventId;
        return this;
    }

    public DayFixture WithMenuId(Guid menuId)
    {
        _menuId = menuId;
        return this;
    }

    public DayFixture WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public DayFixture WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public DayFixture WithSequenceNumber(int number)
    {
        _sequenceNumber = number;
        return this;
    }

    public Day Build()
    {
        return new Day(
            _id ?? Guid.NewGuid(),
            _eventId ?? Guid.NewGuid(),
            _menuId ?? Guid.NewGuid(),
            _title,
            _sequenceNumber,
            _description
        );
    }
}